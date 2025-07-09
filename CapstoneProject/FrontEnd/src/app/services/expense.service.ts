import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment.prod";
import { BehaviorSubject, catchError, map, Observable, tap, throwError } from "rxjs";
import { Expense } from "../models/ExpenseModel";
import { HttpClient, HttpErrorResponse, HttpParams } from "@angular/common/http";
import { ExpenseAddRequestDto, ExpenseFilterOptions, ExpenseUpdateRequestDto } from "../models/ExpenseDTOs";
import { ExpenseSearchModel } from "../models/ExpenseSearchModel";
import { PaginationQuery } from "../models/Pagination";
import { ApiError } from "../models/ApiResponse";

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
    private readonly apiUrl = `${environment.apiBaseUrl}/api/expenses`;
    private readonly receiptApiUrl = `${environment.apiBaseUrl}/api/receipts`;
    private expensesSubject = new BehaviorSubject<Expense[]>([]);
    public expenses$ = this.expensesSubject.asObservable();

    constructor(private http: HttpClient) {}

    private mapExpenseObject(expense: any): Expense {
        console.log('Raw expense object:', expense);
        
        // Handle different property name formats
        const getId = () => expense.id || expense.Id || expense.ID || '';
        const getTitle = () => expense.title || expense.Title || expense.TITLE || '';
        const getCategory = () => expense.category || expense.Category || expense.CATEGORY || '';
        const getNotes = () => expense.notes || expense.Notes || expense.NOTES || '';
        const getAmount = () => {
            const amount = expense.amount || expense.Amount || expense.AMOUNT;
            return amount !== null && amount !== undefined ? parseFloat(amount) : 0;
        };
        const getUsername = () => expense.username || expense.Username || expense.USERNAME || '';
        const getCreatedBy = () => expense.createdBy || expense.CreatedBy || expense.CREATED_BY || '';
        const getUpdatedBy = () => expense.updatedBy || expense.UpdatedBy || expense.UPDATED_BY || '';
        const getCreatedAt = () => {
            const date = expense.createdAt || expense.CreatedAt || expense.CREATED_AT;
            return date ? new Date(date) : new Date();
        };
        const getUpdatedAt = () => {
            const date = expense.updatedAt || expense.UpdatedAt || expense.UPDATED_AT;
            return date ? new Date(date) : new Date();
        };

        const mappedExpense: Expense = {
            id: getId(),
            title: getTitle(),
            category: getCategory(),
            notes: getNotes(),
            amount: getAmount(),
            username: getUsername(),
            createdBy: getCreatedBy(),
            updatedBy: getUpdatedBy(),
            createdAt: getCreatedAt(),
            updatedAt: getUpdatedAt(),
            receipt: (expense.receipt && typeof expense.receipt === 'object' && expense.receipt.id) 
                ? expense.receipt 
                : null
        };
        
        console.log('Mapped expense result:', mappedExpense);
        return mappedExpense;
    }

    private handleExpenseArrayResponse(response: any): Expense[] {
        console.log('Raw API response:', response);
        
        let expenseArray: any[];
        
        if (response && typeof response === 'object' && response.$values) {
            expenseArray = response.$values;
        } else if (Array.isArray(response)) {
            expenseArray = response;
        } else {
            console.error('Invalid response format:', response);
            throw new Error('Invalid response format: expected array of expenses');
        }
        
        if (!Array.isArray(expenseArray)) {
            console.error('Not an array:', expenseArray);
            throw new Error('Invalid response format: expected array');
        }
        
        console.log('Expense array to map:', expenseArray);
        console.log('First expense raw data:', expenseArray[0]);
        
        const mappedExpenses = expenseArray.map((expense, index) => {
            console.log(`Mapping expense ${index}:`, expense);
            const mapped = this.mapExpenseObject(expense);
            console.log(`Mapped expense ${index}:`, mapped);
            return mapped;
        });
        
        return mappedExpenses;
    }

    getAllExpenses(options: ExpenseFilterOptions = {}): Observable<Expense[]> {
        let params = new HttpParams();
        
        if (options.pagination) {
            params = params.set('pageNumber', options.pagination.pageNumber.toString());
            params = params.set('pageSize', options.pagination.pageSize.toString());
        } else {
            params = params.set('pageNumber', '1');
            params = params.set('pageSize', '100');
        }
        
        if (options.username) {
            params = params.set('username', options.username);
        }
        
        const url = `${this.apiUrl}/all`;
        
        return this.http.get<any>(url, { params })
        .pipe(
            map((response: any) => this.handleExpenseArrayResponse(response)),
            tap((expenses: Expense[]) => {
                this.expensesSubject.next(expenses);
            }),
            catchError(this.handleError.bind(this))
        );
    }

    getMyExpenses(pagination: PaginationQuery = { pageNumber: 1, pageSize: 100 }): Observable<Expense[]> {
        return this.getAllExpenses({ pagination });
    }

    advancedSearch(criteria: {
        title?: string;
        category?: string;
        minAmount?: number;
        maxAmount?: number;
        startDate?: Date;
        endDate?: Date;
    }): Observable<Expense[]> {
        const searchModel: ExpenseSearchModel = {
            title: criteria.title,
            category: criteria.category,
            amountRange: (criteria.minAmount !== undefined || criteria.maxAmount !== undefined) ? {
                minVal: criteria.minAmount,
                maxVal: criteria.maxAmount
            } : undefined,
            dateRange: (criteria.startDate || criteria.endDate) ? {
                minVal: criteria.startDate,
                maxVal: criteria.endDate
            } : undefined
        };
        
        const searchPayload = {
            ...searchModel,
            dateRange: searchModel.dateRange ? {
                minVal: searchModel.dateRange.minVal?.toISOString(),
                maxVal: searchModel.dateRange.maxVal?.toISOString()
            } : undefined
        };
        
        const url = `${this.apiUrl}/search`;
        
        return this.http.post<any>(url, searchPayload)
        .pipe(
            map((response: any) => this.handleExpenseArrayResponse(response)),
            catchError(this.handleError.bind(this))
        );
    }

    getExpenseWithReceipt(expenseId: string): Observable<Expense> {
        const url = `${this.apiUrl}/get/${expenseId}?includeReceipt=true`;
        
        return this.http.get<any>(url)
        .pipe(
            map((response: any) => {
                const expense: Expense = {
                    id: response.id,
                    title: response.title,
                    category: response.category,
                    notes: response.notes || '',
                    amount: response.amount,
                    createdAt: new Date(response.createdAt),
                    createdBy: response.createdBy,           
                    updatedAt: new Date(response.updatedAt),
                    updatedBy: response.updatedBy,         
                    username: response.username,
                    receipt: response.receipt ? {
                        id: response.receipt.id,
                        receiptName: response.receipt.receiptName,
                        category: response.receipt.category,
                        createdAt: new Date(response.receipt.createdAt),
                        contentType: response.receipt.contentType,
                        fileSizeBytes: response.receipt.fileSizeBytes
                    } : null
                };
                
                return expense;
            }),
            catchError(this.handleError.bind(this))
        );
    }

    previewReceipt(receiptId: string): Observable<Blob> {
        const url = `${this.receiptApiUrl}/preview/${receiptId}`;
        
        return this.http.get(url, { 
            responseType: 'blob',
            observe: 'response'
        }).pipe(
            map(response => {
                return response.body!;
            }),
            catchError(this.handleError.bind(this))
        );
    }

    downloadReceipt(receiptId: string, fileName: string): void {
        const url = `${this.receiptApiUrl}/download/${receiptId}`;
        
        this.http.get(url, { 
            responseType: 'blob',
            observe: 'response'
        }).subscribe({
            next: (response) => {
                const blob = response.body!;
                const downloadUrl = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = downloadUrl;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                URL.revokeObjectURL(downloadUrl);
            },
            error: (error) => {
                console.error('Download failed:', error);
            }
        });
    }

    addExpense(expenseDto: ExpenseAddRequestDto): Observable<Expense> {
        const formData = new FormData();
        formData.append('Title', expenseDto.title);
        formData.append('Category', expenseDto.category);
        formData.append('Amount', expenseDto.amount.toString());
        
        if (expenseDto.notes?.trim()) {
            formData.append('Notes', expenseDto.notes);
        }
        
        if (expenseDto.targetUsername?.trim()) {
            formData.append('TargetUsername', expenseDto.targetUsername);
        }
        
        if (expenseDto.receiptBill) {
            formData.append('ReceiptBill', expenseDto.receiptBill);
        }

        return this.http.post<any>(`${this.apiUrl}/add`, formData)
        .pipe(
            map(response => this.mapExpenseObject(response)),
            tap((createdExpense: Expense) => {
                const currentExpenses = this.expensesSubject.value;
                this.expensesSubject.next([createdExpense, ...currentExpenses]);
            }),
            catchError(this.handleError.bind(this))
        );
    }

    updateExpense(expenseDto: ExpenseUpdateRequestDto): Observable<Expense> {
        const formData = new FormData();
        formData.append('Id', expenseDto.id);
        
        if (expenseDto.title?.trim()) {
            formData.append('Title', expenseDto.title);
        }
        
        if (expenseDto.category?.trim()) {
            formData.append('Category', expenseDto.category);
        }
        
        if (expenseDto.notes?.trim()) {
            formData.append('Notes', expenseDto.notes);
        }
        
        if (expenseDto.amount !== undefined && expenseDto.amount !== null) {
            formData.append('Amount', expenseDto.amount.toString());
        }
        
        if (expenseDto.receipt) {
            formData.append('Receipt', expenseDto.receipt);
        }
        
        const url = `${this.apiUrl}/update/${expenseDto.id}`;
        
        return this.http.put<any>(url, formData)
        .pipe(
            map(response => this.mapExpenseObject(response)),
            tap((updatedExpense: Expense) => {
                const currentExpenses = this.expensesSubject.value;
                const updatedExpenses = currentExpenses.map(expense => 
                    expense.id === updatedExpense.id ? updatedExpense : expense
                );
                this.expensesSubject.next(updatedExpenses);
            }),
            catchError(this.handleError.bind(this))
        );
    }

    deleteExpense(expenseId: string): Observable<Expense> {
        const url = `${this.apiUrl}/delete/${expenseId}`;
            
        return this.http.delete<Expense>(url)
        .pipe(
            tap((deletedExpense: Expense) => {
                const currentExpenses = this.expensesSubject.value;
                const filteredExpenses = currentExpenses.filter(expense => expense.id !== expenseId);
                this.expensesSubject.next(filteredExpenses);
            }),
            catchError(this.handleError.bind(this))
        );
    }

    private handleError(error: HttpErrorResponse): Observable<never> {
        let apiError: ApiError;
        
        if (error.error instanceof ErrorEvent) {
            apiError = {
                message: 'A network error occurred. Please check your connection.',
                status: 0,
                details: error.error.message
            };
        } else {
            apiError = {
                message: error.error?.message || 'An unknown server error occurred.',
                status: error.status,
                details: error.error
            };
            
            switch (error.status) {
                case 400:
                    apiError.message = error.error?.message || 'Invalid expense data provided.';
                    break;
                case 401:
                    apiError.message = 'You are not authorized to perform this action.';
                    break;
                case 403:
                    apiError.message = 'You do not have permission to access this expense.';
                    break;
                case 404:
                    apiError.message = 'The requested expense was not found.';
                    break;
                case 413:
                    apiError.message = 'File size too large. Please choose a smaller file.';
                    break;
                case 415:
                    apiError.message = 'Unsupported file type. Please upload a valid image or PDF.';
                    break;
                case 500:
                    apiError.message = 'A server error occurred. Please try again later.';
                    break;
            }
        }
        
        return throwError(() => apiError);
    }
}