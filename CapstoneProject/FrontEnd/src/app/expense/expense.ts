import { Component, OnDestroy, OnInit } from '@angular/core';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { CurrentUser } from '../models/CurrentUser';
import { Expense } from '../models/ExpenseModel';
import { ExpenseService } from '../services/expense.service';
import { AuthService } from '../services/auth.service';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EditExpenseComponent } from '../edit-expense/edit-expense';
import { AddExpenseComponent } from '../add-expense/add-expense';
import { ExpenseFilterOptions } from '../models/ExpenseDTOs';

@Component({
  selector: 'app-expense',
  imports: [CommonModule, FormsModule, RouterModule, EditExpenseComponent, AddExpenseComponent],
  templateUrl: './expense.html',
  styleUrl: './expense.css'
})
export class ExpenseComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  currentUser: CurrentUser | null = null;

  // Admin viewing functionality
  isAdminView: boolean = false;
  targetUsername: string | null = null;
  viewingUserDisplayName: string = '';

  expenses: Expense[] = [];
  filteredExpenses: Expense[] = [];
  isLoading: boolean = false;
  isFiltering: boolean = false;
  loadingError: string | null = null;

  searchQuery: string = '';
  private searchSubject = new Subject<string>();
  private filterSubject = new Subject<void>();

  showDateFilter: boolean = false;
  showCategoryFilter: boolean = false;
  startDate: string = '';
  endDate: string = '';
  selectedCategory: string = '';

  currentPage: number = 1;
  pageSize: number = 20;
  totalExpenses: number = 0;
  hasMorePages: boolean = false;

  Math = Math;

  categories = [
    'Food & Dining',
    'Transportation', 
    'Shopping',
    'Entertainment',
    'Health & Medical',
    'Bills & Utilities',
    'Travel',
    'Education',
    'Personal Care',
    'Gifts & Donations'
  ];

  editingExpense: Expense | null = null;
  showEditModal: boolean = false;
  showAddModal: boolean = false;

  showReceiptModal: boolean = false;
  viewingReceiptExpense: Expense | null = null;
  receiptDataUrl: string | null = null;
  isLoadingReceipt: boolean = false;
  receiptError: string | null = null;

  constructor(
    private expenseService: ExpenseService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
    this.checkForAdminView();
    this.setupSearchAndFilters();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.receiptDataUrl) {
      URL.revokeObjectURL(this.receiptDataUrl);
    }
  }

  private checkForAdminView(): void {
    // Check query parameters for admin view
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.isAdminView = params['adminView'] === 'true';
        this.targetUsername = params['targetUser'] || null;
        
        if (this.isAdminView && this.targetUsername) {
          console.log('Admin viewing expenses for user:', this.targetUsername);
          this.viewingUserDisplayName = this.getDisplayName(this.targetUsername);
          
          // Verify current user is admin
          if (this.currentUser?.role !== 'Admin') {
            console.error('Non-admin user trying to access admin view');
            this.router.navigate(['/expenses']);
            return;
          }
        }
        
        // Load expenses after checking admin view
        this.loadUserExpenses();
      });
  }

  private getDisplayName(username: string): string {
    const atIndex = username.indexOf('@');
    return atIndex > -1 ? username.substring(0, atIndex) : username;
  }

  private initializeComponent(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          const previousUsername = this.currentUser?.username;
          this.currentUser = user;
          
          // If in admin view and user role changed, verify still admin
          if (this.isAdminView && user?.role !== 'Admin') {
            console.error('User lost admin privileges, redirecting');
            this.router.navigate(['/expenses']);
            return;
          }
          
          if (user && user.username !== previousUsername && !this.isAdminView) {
            this.resetFiltersAndLoadExpenses();
          } else if (!user && previousUsername) {
            this.expenses = [];
            this.filteredExpenses = [];
            this.loadingError = 'User not authenticated';
          }
        },
        error: (error) => {
          this.loadingError = 'Authentication error';
        }
      });
  }

  private setupSearchAndFilters(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(query => {
      this.performSearch(query);
    });

    this.filterSubject.pipe(
      debounceTime(500),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.currentPage = 1;
      this.loadUserExpenses();
    });
  }

  private loadUserExpenses(): void {
    this.isLoading = true;
    this.loadingError = null;

    if (!this.currentUser) {
      this.currentUser = this.authService.getCurrentUser();
    }

    if (!this.currentUser) {
      this.isLoading = false;
      this.loadingError = 'User not authenticated. Please log in again.';
      return;
    }

    // Determine which user's expenses to load
    const usernameToLoad = this.isAdminView && this.targetUsername ? this.targetUsername : this.currentUser.username;
    
    console.log('Loading expenses for user:', usernameToLoad, 'Admin view:', this.isAdminView);

    const hasDateFilter = this.startDate && this.endDate;
    const hasCategoryFilter = this.selectedCategory;
    const hasAnyFilter = hasDateFilter || hasCategoryFilter;
    
    if (hasAnyFilter) {
      this.isFiltering = true;
      
      // For admin view with filters, use getAllExpenses with filter options
      if (this.isAdminView && this.targetUsername) {
        const filterOptions: ExpenseFilterOptions = {
          pagination: { pageNumber: 1, pageSize: 100 },
          username: this.targetUsername
        };
        
        // Note: The current service doesn't support date/category filtering in getAllExpenses
        // You may need to extend the service or handle filtering client-side
        this.expenseService.getAllExpenses(filterOptions)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (expenses) => {
              // Apply client-side filtering for date and category
              let filteredExpenses = expenses;
              
              if (this.startDate && this.endDate) {
                const startDate = new Date(this.startDate);
                const endDate = new Date(this.endDate);
                filteredExpenses = filteredExpenses.filter(expense => {
                  const expenseDate = new Date(expense.createdAt);
                  return expenseDate >= startDate && expenseDate <= endDate;
                });
              }
              
              if (this.selectedCategory) {
                filteredExpenses = filteredExpenses.filter(expense => 
                  expense.category === this.selectedCategory
                );
              }
              
              this.handleExpensesResponse(filteredExpenses, usernameToLoad);
            },
            error: (error) => this.handleExpensesError(error)
          });
      } else {
        // For regular users, use the existing advancedSearch
        const searchCriteria = this.buildSearchCriteria();
        
        this.expenseService.advancedSearch(searchCriteria)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (expenses) => this.handleExpensesResponse(expenses, usernameToLoad),
            error: (error) => this.handleExpensesError(error)
          });
      }
    } else {
      // For admin view, we need to get expenses for specific user
      if (this.isAdminView && this.targetUsername) {
        this.expenseService.getAllExpenses({ 
          pagination: { pageNumber: 1, pageSize: 100 },
          username: this.targetUsername 
        })
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (expenses) => this.handleExpensesResponse(expenses, usernameToLoad),
            error: (error) => this.handleExpensesError(error)
          });
      } else {
        this.expenseService.getMyExpenses({ pageNumber: 1, pageSize: 100 })
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (expenses) => this.handleExpensesResponse(expenses, usernameToLoad),
            error: (error) => this.handleExpensesError(error)
          });
      }
    }
  }

  private handleExpensesResponse(expenses: Expense[], expectedUsername: string): void {
    console.log('Frontend received expenses:', expenses);
    
    if (Array.isArray(expenses)) {
      // In admin view, allow viewing other users' expenses
      if (!this.isAdminView) {
        const currentUsername = this.currentUser?.username;
        
        if (currentUsername) {
          const unauthorizedExpenses = expenses.filter(expense => 
            expense.username && expense.username !== currentUsername
          );
          
          if (unauthorizedExpenses.length > 0) {
            this.loadingError = 'Security error: Received unauthorized data. Please contact support.';
            this.isLoading = false;
            this.isFiltering = false;
            return;
          }
        }
      }
      
      // Debug each expense
      expenses.forEach((expense, index) => {
        console.log(`Expense ${index}:`, {
          id: expense.id,
          title: expense.title,
          amount: expense.amount,
          category: expense.category,
          createdAt: expense.createdAt,
          hasReceipt: !!expense.receipt,
          username: expense.username
        });
      });
      
      let processedExpenses = expenses;
      if (this.searchQuery.trim()) {
        processedExpenses = this.performClientSideSearch(expenses, this.searchQuery);
      }
      
      this.totalExpenses = processedExpenses.length;
      const startIndex = (this.currentPage - 1) * this.pageSize;
      const endIndex = startIndex + this.pageSize;
      
      this.expenses = processedExpenses;
      this.filteredExpenses = processedExpenses.slice(startIndex, endIndex);
      this.hasMorePages = endIndex < this.totalExpenses;
      
      console.log('Final filtered expenses:', this.filteredExpenses);
      
      this.loadingError = null;
    } else {
      console.error('Expected array but got:', typeof expenses, expenses);
      this.loadingError = 'Invalid data format received from server';
    }
    
    this.isLoading = false;
    this.isFiltering = false;
  }

  private handleExpensesError(error: any): void {
    this.isLoading = false;
    this.isFiltering = false;
    
    if (error.status === 401) {
      this.loadingError = 'Authentication expired. Please log in again.';
      this.authService.logout();
    } else if (error.status === 403) {
      this.loadingError = 'Access denied. You do not have permission to view these expenses.';
    } else if (error.status === 0) {
      this.loadingError = 'Cannot connect to server. Please check your internet connection.';
    } else if (error.status >= 500) {
      this.loadingError = 'Server error. Please try again later.';
    } else {
      this.loadingError = error.message || 'Failed to load expenses. Please try again.';
    }
  }

  private buildSearchCriteria() {
    const criteria: any = {};
    
    if (this.startDate && this.endDate) {
      criteria.startDate = new Date(this.startDate);
      criteria.endDate = new Date(this.endDate);
    }
    
    if (this.selectedCategory) {
      criteria.category = this.selectedCategory;
    }
    
    return criteria;
  }

  private performClientSideSearch(expenses: Expense[], query: string): Expense[] {
    const searchTerm = query.toLowerCase();
    return expenses.filter(expense =>
      expense.title?.toLowerCase().includes(searchTerm) ||
      expense.category?.toLowerCase().includes(searchTerm) ||
      expense.notes?.toLowerCase().includes(searchTerm)
    );
  }

  private performSearch(query: string): void {
    if (!query.trim()) {
      this.applyPagination();
      return;
    }

    const searchResults = this.performClientSideSearch(this.expenses, query);
    this.totalExpenses = searchResults.length;
    this.currentPage = 1;
    const startIndex = 0;
    const endIndex = this.pageSize;
    
    this.filteredExpenses = searchResults.slice(startIndex, endIndex);
    this.hasMorePages = endIndex < this.totalExpenses;
  }

  private applyPagination(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    
    this.filteredExpenses = this.expenses.slice(startIndex, endIndex);
    this.hasMorePages = endIndex < this.totalExpenses;
  }

  hasReceipt(expense: Expense): boolean {
    return !!(expense?.receipt?.id);
  }

  viewReceipt(expense: Expense): void {
    // Allow admin to view any receipt, regular users only their own
    if (!this.isAdminView && this.currentUser?.username && expense.username && expense.username !== this.currentUser.username) {
      alert('You can only view receipts for your own expenses.');
      return;
    }
    
    if (!this.hasReceipt(expense)) {
      alert('This expense does not have a receipt attached.');
      return;
    }
    
    this.isLoadingReceipt = true;
    this.receiptError = null;
    this.viewingReceiptExpense = expense;
    this.showReceiptModal = true;
    
    this.expenseService.getExpenseWithReceipt(expense.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
            next: (expenseWithReceipt) => {
                if (expenseWithReceipt.receipt?.id) {
                    this.viewingReceiptExpense = expenseWithReceipt;
                    this.loadReceiptPreview(expenseWithReceipt.receipt.id);
                } else {
                    this.receiptError = 'No receipt found for this expense.';
                    this.isLoadingReceipt = false;
                }
            },
            error: (error) => {
                this.receiptError = 'Failed to load receipt information.';
                this.isLoadingReceipt = false;
            }
        });
  }

  private loadReceiptPreview(receiptId: string): void {
    this.expenseService.previewReceipt(receiptId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
            next: (blob) => {
                if (this.receiptDataUrl) {
                    URL.revokeObjectURL(this.receiptDataUrl);
                }
                this.receiptDataUrl = URL.createObjectURL(blob);
                this.isLoadingReceipt = false;
            },
            error: (error) => {
                this.receiptError = 'Failed to load receipt preview.';
                this.isLoadingReceipt = false;
            }
        });
  }

  closeReceiptModal(): void {
    this.showReceiptModal = false;
    this.viewingReceiptExpense = null;
    this.receiptError = null;
    this.isLoadingReceipt = false;
    
    if (this.receiptDataUrl) {
      URL.revokeObjectURL(this.receiptDataUrl);
      this.receiptDataUrl = null;
    }
  }

  downloadReceipt(): void {
    if (!this.viewingReceiptExpense?.receipt?.id || !this.viewingReceiptExpense?.receipt?.receiptName) {
      return;
    }
    
    this.expenseService.downloadReceipt(
      this.viewingReceiptExpense.receipt.id, 
      this.viewingReceiptExpense.receipt.receiptName
    );
  }

  getReceiptFileType(): 'image' | 'pdf' | 'other' {
    if (!this.viewingReceiptExpense?.receipt?.contentType) {
      return 'other';
    }
    
    const contentType = this.viewingReceiptExpense.receipt.contentType.toLowerCase();
    
    if (contentType.startsWith('image/')) {
      return 'image';
    } else if (contentType === 'application/pdf') {
      return 'pdf';
    } else {
      return 'other';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchQuery);
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.searchSubject.next('');
  }

  toggleDateFilter(): void {
    this.showDateFilter = !this.showDateFilter;
    if (this.showDateFilter) {
      this.showCategoryFilter = false;
    }
  }

  toggleCategoryFilter(): void {
    this.showCategoryFilter = !this.showCategoryFilter;
    if (this.showCategoryFilter) {
      this.showDateFilter = false;
    }
  }

  onDateRangeChange(): void {
    if (this.startDate && this.endDate) {
      if (new Date(this.startDate) > new Date(this.endDate)) {
        alert('Start date cannot be after end date');
        return;
      }
      this.filterSubject.next();
    } else if (!this.startDate && !this.endDate) {
      this.filterSubject.next();
    }
  }

  clearDateFilter(): void {
    this.startDate = '';
    this.endDate = '';
    this.filterSubject.next();
  }

  onCategoryChange(): void {
    this.filterSubject.next();
  }

  clearCategoryFilter(): void {
    this.selectedCategory = '';
    this.filterSubject.next();
  }

  clearAllFilters(): void {
    this.startDate = '';
    this.endDate = '';
    this.selectedCategory = '';
    this.searchQuery = '';
    this.currentPage = 1;
    this.filterSubject.next();
  }

  private resetFiltersAndLoadExpenses(): void {
    this.clearAllFilters();
    this.loadUserExpenses();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
      if (this.searchQuery.trim()) {
        this.performSearch(this.searchQuery);
      } else {
        this.applyPagination();
      }
    }
  }

  nextPage(): void {
    if (this.hasMorePages) {
      this.currentPage++;
      if (this.searchQuery.trim()) {
        this.performSearch(this.searchQuery);
      } else {
        this.applyPagination();
      }
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      if (this.searchQuery.trim()) {
        this.performSearch(this.searchQuery);
      } else {
        this.applyPagination();
      }
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalExpenses / this.pageSize);
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(totalPages, start + maxVisible - 1);
    
    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  refreshExpenses(): void {
    this.loadingError = null;
    this.loadUserExpenses();
  }

  navigateToAddExpense(): void {
    this.showAddModal = true;
  }

  editExpense(expense: Expense): void {
    // Allow admin to edit any expense, regular users only their own
    if (!this.isAdminView && this.currentUser?.username && expense.username && expense.username !== this.currentUser.username) {
      alert('You can only edit your own expenses.');
      return;
    }
    
    this.editingExpense = { ...expense };
    this.showEditModal = true;
  }

  deleteExpense(expense: Expense): void {
    // Allow admin to delete any expense, regular users only their own
    if (!this.isAdminView && this.currentUser?.username && expense.username && expense.username !== this.currentUser.username) {
      alert('You can only delete your own expenses.');
      return;
    }
    
    const ownerName = this.isAdminView ? this.viewingUserDisplayName : 'your';
    if (confirm(`Are you sure you want to delete "${expense.title}" from ${ownerName} expenses?`)) {
      this.expenseService.deleteExpense(expense.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadUserExpenses();
          },
          error: (error) => {
            alert('Failed to delete expense. Please try again.');
          }
        });
    }
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editingExpense = null;
  }

  closeAddModal(): void {
    this.showAddModal = false;
  }

  onExpenseUpdated(updatedExpense: Expense): void {
    this.closeEditModal();
    this.loadUserExpenses();
  }

  onExpenseAdded(newExpense: Expense): void {
    this.closeAddModal();
    this.loadUserExpenses();
  }

  // Navigation methods
  goBackToUserManagement(): void {
    if (this.isAdminView) {
      this.router.navigate(['/admin-home']);
    }
  }

  logout(): void {
    this.authService.logout();
  }

  getCategoryColor(category: string): string {
    const categoryColors: { [key: string]: string } = {
      'Food & Dining': '#6366f1',
      'Transportation': '#10b981',
      'Shopping': '#f59e0b',
      'Entertainment': '#ef4444',
      'Health & Medical': '#8b5cf6',
      'Bills & Utilities': '#06b6d4',
      'Travel': '#84cc16',
      'Education': '#f97316',
      'Personal Care': '#ec4899',
      'Gifts & Donations': '#14b8a6'
    };
    return categoryColors[category] || '#6b7280';
  }

  formatDate(date: Date | string): string {
    
    
    try {
      const dateObj = date instanceof Date ? date : new Date(date);
      
      if (isNaN(dateObj.getTime())) {
        console.warn('Invalid date:', date);
        return 'Invalid Date';
      }
      
      return dateObj.toLocaleDateString('en-GB', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  formatAmount(amount: number): string {
    
    // Handle null, undefined, or NaN values
    if (amount === null || amount === undefined || isNaN(amount) || amount === 0) {
      return '₹ 0.00';
    }
    
    // Convert to number if it's a string
    const numAmount = typeof amount === 'string' ? parseFloat(amount) : amount;
    
    if (isNaN(numAmount)) {
      console.warn('Could not parse amount:', amount);
      return '₹ 0.00';
    }
    
    try {
      return new Intl.NumberFormat('en-IN', {
        currencySign: 'standard',
        style: 'currency',
        currency: 'INR'
      }).format(numAmount);
    } catch (error) {
      console.error('Error formatting amount:', error);
      return '₹ 0.00';
    }
  }

  trackByExpenseId(index: number, expense: Expense): string {
    return expense.id;
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.startDate && this.endDate) count++;
    if (this.selectedCategory) count++;
    return count;
  }

  hasActiveFilters(): boolean {
    return this.getActiveFilterCount() > 0 || this.searchQuery.trim().length > 0;
  }

  hasDateFilter(): boolean {
    return !!this.startDate && !!this.endDate;
  }

  hasCategoryFilter(): boolean {
    return !!this.selectedCategory;
  }

  getFormattedDateRange(): string {
    if (this.startDate && this.endDate) {
      const start = new Date(this.startDate).toLocaleDateString('en-GB', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
      });
      const end = new Date(this.endDate).toLocaleDateString('en-GB', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
      });
      return `${start} to ${end}`;
    }
    return '';
  }

  getExpensesWithReceiptsCount(): number {
    return this.filteredExpenses.filter(expense => this.hasReceipt(expense)).length;
  }

  // Computed properties for admin view
  get pageTitle(): string {
    if (this.isAdminView && this.targetUsername) {
      return `${this.viewingUserDisplayName}'s Expenses`;
    }
    return 'My Expenses';
  }

  get canCreateExpense(): boolean {
    // For now, let's say admins can create expenses for users
    return true;
  }
}