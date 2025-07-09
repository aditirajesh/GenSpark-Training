import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment.prod";
import { HttpClient, HttpErrorResponse, HttpParams } from "@angular/common/http";
import { CategoryBreakdownDto, DetailedReportDto, QuickSummaryRequestDto, ReportRequestDto, ReportSummaryDto, TimeBasedReportDto, TopExpenseDto } from "../models/ReportDTO";
import { catchError, map, Observable, throwError } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/reports`;

  constructor(private http: HttpClient) {}

  // FIXED: Added username parameter support for quick summary
  getQuickSummary(request: QuickSummaryRequestDto, username?: string): Observable<ReportSummaryDto> {
    let params = new HttpParams();
    
    if (username) {
      params = params.set('username', username);
    }

    return this.http.post<ReportSummaryDto>(`${this.apiUrl}/quick-summary`, request, { params })
      .pipe(catchError(this.handleError.bind(this)));
  }

  getCategoryBreakdown(startDate: string, endDate: string, username?: string): Observable<CategoryBreakdownDto[]> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);
    
    if (username) {
      params = params.set('username', username);
    }

    return this.http.get<any>(`${this.apiUrl}/category-breakdown`, { params })
      .pipe(
        map((response: any): CategoryBreakdownDto[] => {
          if (response && typeof response === 'object' && response.$values) {
            return response.$values;
          } else if (Array.isArray(response)) {
            return response;
          }
          return [];
        }),
        catchError(this.handleError.bind(this))
      );
  }

  getTimeBasedReport(startDate: string, endDate: string, groupBy: string = 'month', username?: string): Observable<TimeBasedReportDto[]> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate)
      .set('groupBy', groupBy);
    
    if (username) {
      params = params.set('username', username);
    }

    return this.http.get<any>(`${this.apiUrl}/time-based`, { params })
      .pipe(
        map((response: any): TimeBasedReportDto[] => {
          if (response && typeof response === 'object' && response.$values) {
            return response.$values;
          } else if (Array.isArray(response)) {
            return response;
          }
          return [];
        }),
        catchError(this.handleError.bind(this))
      );
  }

  getTopExpenses(startDate: string, endDate: string, limit: number = 10, username?: string): Observable<TopExpenseDto[]> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate)
      .set('limit', limit.toString());
    
    if (username) {
      params = params.set('username', username);
    }

    return this.http.get<any>(`${this.apiUrl}/top-expenses`, { params })
      .pipe(
        map((response: any): TopExpenseDto[] => {
          if (response && typeof response === 'object' && response.$values) {
            return response.$values;
          } else if (Array.isArray(response)) {
            return response;
          }
          return [];
        }),
        catchError(this.handleError.bind(this))
      );
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR'
    }).format(amount);
  }

  formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  getDateRange(days: number): { startDate: string, endDate: string } {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - days);
    
    return {
      startDate: this.formatDate(startDate),
      endDate: this.formatDate(endDate)
    };
  }

  downloadReportAsJSON(data: any, filename: string): void {
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    this.downloadBlob(blob, `${filename}.json`);
  }

  private downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Network error: ${error.error.message}`;
    } else {
      switch (error.status) {
        case 400:
          errorMessage = error.error?.message || 'Invalid request parameters';
          break;
        case 401:
          errorMessage = 'You are not authorized to access these reports';
          break;
        case 403:
          errorMessage = 'You do not have permission to access this report';
          break;
        case 404:
          errorMessage = 'Report not found';
          break;
        case 500:
          errorMessage = 'Server error. Please try again later';
          break;
        default:
          errorMessage = error.error?.message || `Server error: ${error.status}`;
      }
    }
    
    return throwError(() => new Error(errorMessage));
  }
}