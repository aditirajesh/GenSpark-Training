export interface QuickSummaryRequestDto {
  username?: string;
  lastNDays: number;
}

export interface ReportRequestDto {
  startDate: string;
  endDate: string;
  username?: string;
  category?: string;
  topExpensesLimit: number;
}

export interface ReportSummaryDto {
  reportType: string;
  username: string;
  createdAt: string;
  createdBy: string;
  timeline: string;
  totalExpense: number;
  totalExpenseCount: number;
  averageExpenseAmount: number;
  topCategory: string;
  topCategoryAmount: number;
  period: string;
}

export interface CategoryBreakdownDto {
  category: string;
  totalAmount: number;
  expenseCount: number;
  percentage: number;
  averageAmount: number;
}

export interface TimeBasedReportDto {
  timePeriod: string;
  totalAmount: number;
  expenseCount: number;
  averageAmount: number;
  topCategories: CategoryBreakdownDto[];
}

export interface TopExpenseDto {
  expenseId: string;
  title: string;
  category: string;
  amount: number;
  createdAt: string;
  notes: string;
  hasReceipt: boolean;
}

export interface DetailedReportDto {
  summary: ReportSummaryDto;
  categoryBreakdown: CategoryBreakdownDto[];
  timeBasedData: TimeBasedReportDto[];
  topExpenses: TopExpenseDto[];
}
