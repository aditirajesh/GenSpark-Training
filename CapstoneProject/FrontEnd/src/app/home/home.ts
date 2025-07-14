import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ReportsService } from '../services/report.service';
import { CurrentUser } from '../models/CurrentUser';
import { CategoryBreakdownDto, TopExpenseDto } from '../models/ReportDTO';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  @ViewChild('categoryChart') categoryChartRef!: ElementRef<HTMLCanvasElement>;
  
  private destroy$ = new Subject<void>();
  private categoryChart?: Chart;
  
  // User and Authentication
  currentUser: CurrentUser | null = null;
  
  // Loading states
  isLoadingData: boolean = true;
  isNavigatingToAddExpense: boolean = false;
  isNavigatingToReports: boolean = false;
  
  // Real data from ReportsService
  monthlySpending: number = 0;
  dailyAverage: number = 0;
  totalExpenses: number = 0;
  monthProgress: number = 0;
  
  // Charts and data
  categoryBreakdown: CategoryBreakdownDto[] = [];
  topExpenses: TopExpenseDto[] = [];
  
  // Date information
  currentMonth: string = '';
  daysIntoMonth: number = 0;
  
  // Chart colors
  colors = {
    gradients: [
      '#6366f1', '#8b5cf6', '#06b6d4', '#10b981', 
      '#f59e0b', '#ef4444', '#ec4899', '#14b8a6',
      '#84cc16', '#f97316'
    ]
  };

  constructor(
    private authService: AuthService,
    private reportsService: ReportsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
    this.calculateDateInfo();
  }

  ngOnDestroy(): void {
    this.destroyChart();
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeComponent(): void {
    // Subscribe to current user
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (user) {
          this.loadDashboardData();
        } else {
          this.isLoadingData = false;
        }
      });
  }

  private async loadDashboardData(): Promise<void> {
    if (!this.currentUser) {
      this.isLoadingData = false;
      return;
    }

    try {
      this.isLoadingData = true;
      
      // Get current month date range - Fixed to ensure only current month data
      const now = new Date();
      
      // Start of current month at 00:00:00
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
      firstDay.setHours(0, 0, 0, 0);
      
      // End of current month at 23:59:59
      const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      lastDay.setHours(23, 59, 59, 999);
      
      // Format dates ensuring only current month is included
      const startDate = this.formatDateForAPI(firstDay);
      const endDate = this.formatDateForAPI(lastDay);
      
      console.log('Loading dashboard data for current month only:', startDate, 'to', endDate);
      console.log('Current date:', now);
      console.log('Month boundaries:', firstDay, 'to', lastDay);
      
      // Load data from ReportsService
      const [categoryData, topExpensesData] = await Promise.all([
        this.reportsService.getCategoryBreakdown(startDate, endDate).toPromise(),
        this.reportsService.getTopExpenses(startDate, endDate, 3).toPromise()
      ]);
      
      // Process category data
      this.categoryBreakdown = Array.isArray(categoryData) ? categoryData : [];
      
      // Process top expenses data and filter to ensure only current month expenses
      const rawTopExpenses = Array.isArray(topExpensesData) ? topExpensesData : [];
      this.topExpenses = this.filterCurrentMonthExpenses(rawTopExpenses);
      
      // Calculate metrics
      this.calculateMetrics();
      
      // Create chart after data is loaded
      setTimeout(() => {
        this.createCategoryChart();
      }, 100);
      
      console.log('Dashboard data loaded successfully');
      console.log('Top expenses after filtering:', this.topExpenses);
      
    } catch (error) {
      console.error('Error loading dashboard data:', error);
      this.monthlySpending = 0;
      this.dailyAverage = 0;
      this.totalExpenses = 0;
    } finally {
      this.isLoadingData = false;
    }
  }

  private formatDateForAPI(date: Date): string {
    // Format as YYYY-MM-DD to ensure consistent date handling
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private filterCurrentMonthExpenses(expenses: TopExpenseDto[]): TopExpenseDto[] {
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = now.getMonth();
    
    return expenses.filter(expense => {
      const expenseDate = new Date(expense.createdAt);
      return expenseDate.getFullYear() === currentYear && 
             expenseDate.getMonth() === currentMonth;
    });
  }

  private calculateMetrics(): void {
    if (this.categoryBreakdown.length > 0) {
      // Calculate total spending from category breakdown
      this.monthlySpending = this.categoryBreakdown.reduce((sum, category) => sum + category.totalAmount, 0);
      this.totalExpenses = this.categoryBreakdown.reduce((sum, category) => sum + category.expenseCount, 0);
      
      // Calculate daily average
      this.dailyAverage = this.daysIntoMonth > 0 ? this.monthlySpending / this.daysIntoMonth : 0;
    } else {
      this.monthlySpending = 0;
      this.totalExpenses = 0;
      this.dailyAverage = 0;
    }
    
    console.log('Calculated metrics:', {
      monthlySpending: this.monthlySpending,
      totalExpenses: this.totalExpenses,
      dailyAverage: this.dailyAverage
    });
  }

  private calculateDateInfo(): void {
    const now = new Date();
    const currentMonth = now.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
    const dayOfMonth = now.getDate();
    
    this.currentMonth = currentMonth;
    this.daysIntoMonth = dayOfMonth;
    
    const daysInCurrentMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0).getDate();
    this.monthProgress = Math.round((dayOfMonth / daysInCurrentMonth) * 100);
  }

  private createCategoryChart(): void {
    if (!this.categoryChartRef?.nativeElement || !this.categoryBreakdown.length) {
      console.log('Cannot create chart: no canvas element or no data');
      return;
    }

    const ctx = this.categoryChartRef.nativeElement.getContext('2d');
    if (!ctx) {
      console.log('Cannot get canvas context');
      return;
    }

    // Destroy existing chart
    this.destroyChart();

    console.log('Creating category chart with data:', this.categoryBreakdown);

    this.categoryChart = new Chart(ctx, {
      type: 'pie',
      data: {
        labels: this.categoryBreakdown.map(c => c.category),
        datasets: [{
          data: this.categoryBreakdown.map(c => c.totalAmount),
          backgroundColor: this.colors.gradients.slice(0, this.categoryBreakdown.length),
          borderWidth: 2,
          borderColor: '#ffffff'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom' as const,
            labels: {
              usePointStyle: true,
              padding: 10,
              font: {
                size: 11
              }
            }
          },
          tooltip: {
            callbacks: {
              label: (context) => {
                const label = context.label || '';
                const value = this.reportsService.formatCurrency(context.parsed as number);
                const percentage = this.categoryBreakdown[context.dataIndex]?.percentage || 0;
                return `${label}: ${value} (${percentage.toFixed(1)}%)`;
              }
            }
          }
        }
      }
    });
  }

  private destroyChart(): void {
    if (this.categoryChart) {
      this.categoryChart.destroy();
      this.categoryChart = undefined;
    }
  }

  async navigateToAddExpense(): Promise<void> {
    console.log('Navigating to add expense...');
    this.isNavigatingToAddExpense = true;
    
    // Show loading for a brief moment for better UX
    setTimeout(async () => {
      try {
        await this.router.navigate(['/expenses']);
        // The add expense modal will be triggered in the expenses component
      } catch (error) {
        console.error('Navigation error:', error);
      } finally {
        this.isNavigatingToAddExpense = false;
      }
    }, 500);
  }

  async navigateToReports(): Promise<void> {
    console.log('Navigating to reports...');
    this.isNavigatingToReports = true;
    
    // Show loading for a brief moment for better UX
    setTimeout(async () => {
      try {
        await this.router.navigate(['/reports']);
      } catch (error) {
        console.error('Navigation error:', error);
      } finally {
        this.isNavigatingToReports = false;
      }
    }, 800);
  }

  // Logout
  logout(): void {
    console.log('User logging out...');
    this.authService.logout();
  }

  // Helper methods for template
  get formattedMonthlySpending(): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0
    }).format(this.monthlySpending);
  }

  get formattedDailyAverage(): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR'
    }).format(this.dailyAverage);
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR'
    }).format(amount);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short'
    });
  }

  getCategoryColor(index: number): string {
    return this.colors.gradients[index % this.colors.gradients.length];
  }

  get displayUsername(): string {
    if (!this.currentUser?.username) return 'User';
    
    // Remove @example.com or any domain from username
    const username = this.currentUser.username;
    const atIndex = username.indexOf('@');
    return atIndex > -1 ? username.substring(0, atIndex) : username;
  }

  trackByExpenseId(index: number, expense: TopExpenseDto): string {
    return expense.expenseId || index.toString();
  }
}