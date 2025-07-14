
import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';
import { Subject, takeUntil } from 'rxjs';
import { CurrentUser } from '../models/CurrentUser';
import { CategoryBreakdownDto, ReportSummaryDto, TimeBasedReportDto, TopExpenseDto } from '../models/ReportDTO';
import { ReportsService } from '../services/report.service';
import { AuthService } from '../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

Chart.register(...registerables);

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './report.html',
  styleUrl: './report.css'
})
export class ReportComponent implements OnInit, OnDestroy {
  @ViewChild('categoryChart') categoryChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('timeChart') timeChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('topExpensesChart') topExpensesChartRef!: ElementRef<HTMLCanvasElement>;

  private destroy$ = new Subject<void>();
  private categoryChart?: Chart;
  private timeChart?: Chart;
  private topExpensesChart?: Chart;

  currentUser: CurrentUser | null = null;
  isLoading = false;
  error = '';

  // Admin view properties
  isAdminView = false;
  targetUser = '';
  adminUser: CurrentUser | null = null;

  activePreset = 'thisyear';
  customDateRange = {
    startDate: '',
    endDate: ''
  };

  presets = [
    { id: 'last7days', label: 'Last 7 Days', days: 7 },
    { id: 'currentmonth', label: 'Current Month', days: 0 },
    { id: 'last90days', label: 'Last 90 Days', days: 90 },
    { id: 'thisyear', label: 'This Year', days: 0 },
    { id: 'custom', label: 'Custom Range', days: 0 }
  ];

  activeTab = 'overview';
  tabs = [
    { id: 'overview', label: 'Overview' },
    { id: 'categories', label: 'Categories' },
    { id: 'trends', label: 'Trends' },
    { id: 'topexpenses', label: 'Top Expenses' }
  ];

  // Data
  summary: ReportSummaryDto | null = null;
  categoryBreakdown: CategoryBreakdownDto[] = [];
  timeBasedData: TimeBasedReportDto[] = [];
  topExpenses: TopExpenseDto[] = [];

  // Calculated metrics
  totalCost = 0;
  averageCost = 0;
  totalExpenses = 0;
  topCategory = '';
  topCategoryAmount = 0;
  dateRangeText = '';

  colors = {
    gradients: [
      '#6366f1', '#8b5cf6', '#06b6d4', '#10b981', 
      '#f59e0b', '#ef4444', '#ec4899', '#14b8a6',
      '#84cc16', '#f97316'
    ]
  };

  constructor(
    private reportsService: ReportsService,
    public authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (!this.currentUser) {
      this.error = 'Please log in to view reports';
      return;
    }

    // Check for admin view parameters
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      this.isAdminView = params['adminView'] === 'true';
      this.targetUser = params['targetUser'] || '';
      
      if (this.isAdminView) {
        // Store the admin user separately
        this.adminUser = this.currentUser;
        
        // Verify admin permissions
        if (!this.adminUser || this.adminUser.role !== 'Admin') {
          this.router.navigate(['/home']);
          return;
        }
        
        // If no target user specified, redirect back to admin home
        if (!this.targetUser) {
          this.router.navigate(['/admin-home']);
          return;
        }
        
        console.log('Admin view - Loading reports for user:', this.targetUser);
      } else {
        console.log('Regular view - Loading reports for current user:', this.currentUser?.username);
      }
      
      this.loadReports();
    });
  }

  ngOnDestroy(): void {
    this.destroyCharts();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Check if current user is admin
  get isAdmin(): boolean {
    return this.currentUser?.role === 'Admin';
  }

  // Get the username for API calls
  get effectiveUsername(): string {
    return this.isAdminView ? this.targetUser : (this.currentUser?.username || '');
  }

  // Navigation methods (unchanged)
  navigateToHome(): void {
    if (this.isAdmin) {
      this.router.navigate(['/admin-home']);
    } else {
      this.router.navigate(['/home']);
    }
  }

  navigateToExpenses(): void {
    this.router.navigate(['/expenses']);
  }

  navigateToReports(): void {
    this.router.navigate(['/reports']);
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }

  navigateToAdminHome(): void {
    this.router.navigate(['/admin-home']);
  }

  backToUserManagement(): void {
    this.router.navigate(['/admin-home']);
  }

  onPresetChange(presetId: string): void {
    this.activePreset = presetId;
    if (presetId !== 'custom') {
      this.loadReports();
    }
  }

  onCustomDateChange(): void {
    if (this.customDateRange.startDate && this.customDateRange.endDate) {
      this.activePreset = 'custom';
      this.loadReports();
    }
  }

  onTabChange(tabId: string): void {
    this.activeTab = tabId;
    setTimeout(() => this.createCharts(), 100);
  }

  async loadReports(): Promise<void> {
    if (!this.validateDateRange()) return;

    this.isLoading = true;
    this.error = '';

    try {
      const { startDate, endDate, isPreset, days } = this.getDateRange();
      this.updateDateRangeText(startDate, endDate);

      console.log('Loading reports for user:', this.effectiveUsername);
      console.log('Date range:', { startDate, endDate });

      if (isPreset && (this.activePreset === 'last7days' || this.activePreset === 'last90days')) {
        await this.loadPresetData(days);
      } else {
        await this.loadCustomRangeData(startDate, endDate);
      }

      this.calculateMetrics();
      setTimeout(() => this.createCharts(), 100);

    } catch (error: any) {
      console.error('Error loading reports:', error);
      this.error = error.message || 'Failed to load reports';
    } finally {
      this.isLoading = false;
    }
  }

  // FIXED: Pass username parameter to service methods
  private async loadPresetData(days: number): Promise<void> {
    // For preset data, we need to use the date range approach since 
    // quick summary doesn't support username parameter
    const { startDate, endDate } = this.getDateRange();
    await this.loadIndividualData(startDate, endDate);
  }

  private async loadCustomRangeData(startDate: string, endDate: string): Promise<void> {
    await this.loadIndividualData(startDate, endDate);
    
    // Calculate summary from individual data
    if (this.categoryBreakdown.length > 0) {
      const totalAmount = this.categoryBreakdown.reduce((sum, cat) => sum + cat.totalAmount, 0);
      const totalCount = this.categoryBreakdown.reduce((sum, cat) => sum + cat.expenseCount, 0);
      const topCat = this.categoryBreakdown.reduce((prev, current) => 
        current.totalAmount > prev.totalAmount ? current : prev
      );

      this.summary = {
        reportType: 'Custom Range Report',
        username: this.effectiveUsername,
        createdAt: new Date().toISOString(),
        createdBy: this.currentUser?.username || '',
        timeline: `${startDate} to ${endDate}`,
        totalExpense: totalAmount,
        totalExpenseCount: totalCount,
        averageExpenseAmount: totalCount > 0 ? totalAmount / totalCount : 0,
        topCategory: topCat.category,
        topCategoryAmount: topCat.totalAmount,
        period: this.getPresetLabel()
      } as ReportSummaryDto;
    } else {
      // No expenses found - create empty summary
      this.summary = {
        reportType: 'Custom Range Report',
        username: this.effectiveUsername,
        createdAt: new Date().toISOString(),
        createdBy: this.currentUser?.username || '',
        timeline: `${startDate} to ${endDate}`,
        totalExpense: 0,
        totalExpenseCount: 0,
        averageExpenseAmount: 0,
        topCategory: 'No expenses present',
        topCategoryAmount: 0,
        period: this.getPresetLabel()
      } as ReportSummaryDto;
    }
  }

  // FIXED: Pass username parameter to all service calls
  private async loadIndividualData(startDate: string, endDate: string): Promise<void> {
    try {
      console.log('Loading individual data for user:', this.effectiveUsername);
      
      // Pass the username parameter to all service calls
      const [categories, timeBased, topExp] = await Promise.all([
        this.reportsService.getCategoryBreakdown(startDate, endDate, this.effectiveUsername).toPromise(),
        this.reportsService.getTimeBasedReport(startDate, endDate, 'month', this.effectiveUsername).toPromise(),
        this.reportsService.getTopExpenses(startDate, endDate, 10, this.effectiveUsername).toPromise()
      ]);

      this.categoryBreakdown = Array.isArray(categories) ? categories : [];
      this.timeBasedData = Array.isArray(timeBased) ? timeBased : [];
      this.topExpenses = Array.isArray(topExp) ? topExp : [];
      
      console.log('Loaded data:', {
        categories: this.categoryBreakdown.length,
        timeBased: this.timeBasedData.length,
        topExpenses: this.topExpenses.length
      });
      
    } catch (error) {
      console.error('Error loading individual data:', error);
      this.categoryBreakdown = [];
      this.timeBasedData = [];
      this.topExpenses = [];
    }
  }

  private getDateRange(): { startDate: string, endDate: string, isPreset: boolean, days: number } {
    if (this.activePreset === 'custom') {
      return {
        startDate: this.customDateRange.startDate,
        endDate: this.customDateRange.endDate,
        isPreset: false,
        days: 0
      };
    }

    const preset = this.presets.find(p => p.id === this.activePreset);
    
    if (preset?.id === 'currentmonth') {
      const now = new Date();
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
      const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      return {
        startDate: this.reportsService.formatDate(firstDay),
        endDate: this.reportsService.formatDate(lastDay),
        isPreset: false,
        days: 0
      };
    }

    if (preset?.id === 'thisyear') {
      const now = new Date();
      const firstDay = new Date(now.getFullYear(), 0, 1);
      const lastDay = new Date(now.getFullYear(), 11, 31);
      return {
        startDate: this.reportsService.formatDate(firstDay),
        endDate: this.reportsService.formatDate(lastDay),
        isPreset: false,
        days: 0
      };
    }

    if (preset?.days && preset.days > 0) {
      const range = this.reportsService.getDateRange(preset.days);
      return {
        startDate: range.startDate,
        endDate: range.endDate,
        isPreset: true,
        days: preset.days
      };
    }

    // Default to this year
    const now = new Date();
    return {
      startDate: this.reportsService.formatDate(new Date(now.getFullYear(), 0, 1)),
      endDate: this.reportsService.formatDate(new Date(now.getFullYear(), 11, 31)),
      isPreset: false,
      days: 0
    };
  }

  private validateDateRange(): boolean {
    if (this.activePreset === 'custom') {
      if (!this.customDateRange.startDate || !this.customDateRange.endDate) {
        this.error = 'Please select both start and end dates';
        return false;
      }

      const startDate = new Date(this.customDateRange.startDate);
      const endDate = new Date(this.customDateRange.endDate);

      if (startDate > endDate) {
        this.error = 'Start date cannot be after end date';
        return false;
      }

      const daysDiff = Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
      if (daysDiff > 365) {
        this.error = 'Date range cannot exceed 365 days';
        return false;
      }
    }

    this.error = '';
    return true;
  }

  private calculateMetrics(): void {
    if (this.summary) {
      this.totalCost = this.summary.totalExpense;
      this.averageCost = this.summary.averageExpenseAmount;
      this.totalExpenses = this.summary.totalExpenseCount;
      this.topCategory = this.summary.topCategory;
      this.topCategoryAmount = this.summary.topCategoryAmount;
    } else {
      this.totalCost = 0;
      this.averageCost = 0;
      this.totalExpenses = 0;
      this.topCategory = 'No expenses present';
      this.topCategoryAmount = 0;
    }

    // Additional check for empty data state
    if (this.totalExpenses === 0) {
      this.topCategory = 'No expenses present';
      this.topCategoryAmount = 0;
    }
  }

  private updateDateRangeText(startDate: string, endDate: string): void {
    const start = new Date(startDate);
    const end = new Date(endDate);
    
    this.dateRangeText = `${start.toLocaleDateString('en-GB')} to ${end.toLocaleDateString('en-GB')}`;
  }

  private createCharts(): void {
    this.destroyCharts();
    
    if (this.activeTab === 'overview' || this.activeTab === 'categories') {
      this.createCategoryChart();
    }
    
    if (this.activeTab === 'overview' || this.activeTab === 'trends') {
      this.createTrendsChart();
    }
    
    if (this.activeTab === 'overview' || this.activeTab === 'topexpenses') {
      this.createTopExpensesChart();
    }
  }

  private createCategoryChart(): void {
    if (!this.categoryChartRef?.nativeElement || !this.categoryBreakdown.length) return;

    const ctx = this.categoryChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    this.categoryChart = new Chart(ctx, {
      type: 'pie',
      data: {
        labels: this.categoryBreakdown.map(c => c.category),
        datasets: [{
          data: this.categoryBreakdown.map(c => c.totalAmount),
          backgroundColor: this.colors.gradients,
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
              padding: 15
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

  private createTrendsChart(): void {
    if (!this.timeChartRef?.nativeElement || !this.timeBasedData.length) return;

    const ctx = this.timeChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    const labels = this.timeBasedData.map(t => this.formatMonthLabel(t.timePeriod));

    this.timeChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [{
          label: 'Monthly Spending',
          data: this.timeBasedData.map(t => t.totalAmount),
          borderColor: '#6366f1',
          backgroundColor: 'rgba(99, 102, 241, 0.1)',
          borderWidth: 3,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#6366f1',
          pointBorderColor: '#ffffff',
          pointBorderWidth: 2,
          pointRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: (value) => this.reportsService.formatCurrency(value as number)
            }
          }
        },
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (context) => {
                const monthData = this.timeBasedData[context.dataIndex];
                return [
                  `Amount: ${this.reportsService.formatCurrency(context.parsed.y)}`,
                  `Expenses: ${monthData?.expenseCount || 0}`
                ];
              }
            }
          }
        }
      }
    });
  }

  private createTopExpensesChart(): void {
    if (!this.topExpensesChartRef?.nativeElement || !this.topExpenses.length) return;

    const ctx = this.topExpensesChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    const chartData = this.topExpenses.slice(0, 5);

    this.topExpensesChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: chartData.map(e => e.title.length > 15 ? e.title.substring(0, 15) + '...' : e.title),
        datasets: [{
          label: 'Amount',
          data: chartData.map(e => e.amount),
          backgroundColor: this.colors.gradients.slice(0, chartData.length),
          borderRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        indexAxis: 'y' as const,
        scales: {
          x: {
            beginAtZero: true,
            ticks: {
              callback: (value) => this.reportsService.formatCurrency(value as number)
            }
          }
        },
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (context) => {
                const expense = chartData[context.dataIndex];
                return [
                  `Amount: ${this.reportsService.formatCurrency(context.parsed.x)}`,
                  `Category: ${expense.category}`
                ];
              }
            }
          }
        }
      }
    });
  }

  private formatMonthLabel(timePeriod: string): string {
    if (timePeriod.match(/^\d{4}-\d{2}$/)) {
      const [year, month] = timePeriod.split('-');
      const monthNames = [
        'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
        'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
      ];
      return `${monthNames[parseInt(month) - 1]} ${year}`;
    }
    return timePeriod;
  }

  private destroyCharts(): void {
    if (this.categoryChart) {
      this.categoryChart.destroy();
      this.categoryChart = undefined;
    }
    if (this.timeChart) {
      this.timeChart.destroy();
      this.timeChart = undefined;
    }
    if (this.topExpensesChart) {
      this.topExpensesChart.destroy();
      this.topExpensesChart = undefined;
    }
  }

  formatCurrency(amount: number): string {
    return this.reportsService.formatCurrency(amount);
  }

  getPresetLabel(): string {
    const preset = this.presets.find(p => p.id === this.activePreset);
    return preset?.label || 'Custom Range';
  }

  downloadReport(): void {
    const reportData = {
      summary: this.summary,
      categoryBreakdown: this.categoryBreakdown,
      timeBasedData: this.timeBasedData,
      topExpenses: this.topExpenses,
      metrics: {
        totalCost: this.totalCost,
        averageCost: this.averageCost,
        totalExpenses: this.totalExpenses,
        topCategory: this.topCategory,
        dateRange: this.dateRangeText
      },
      generatedAt: new Date().toISOString(),
      viewedBy: this.isAdminView ? this.adminUser?.username : this.currentUser?.username,
      targetUser: this.isAdminView ? this.targetUser : this.currentUser?.username
    };

    const filename = `expense-report-${this.isAdminView ? this.targetUser : 'personal'}-${this.activePreset}-${new Date().toISOString().split('T')[0]}`;
    this.reportsService.downloadReportAsJSON(reportData, filename);
  }

  logout(): void {
    this.authService.logout();
  }
}