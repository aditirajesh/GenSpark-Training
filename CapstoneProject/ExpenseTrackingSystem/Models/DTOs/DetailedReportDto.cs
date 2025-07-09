namespace ExpenseTrackingSystem.Models.DTOs
{
    public class DetailedReportDto
    {
        public ReportSummaryDto Summary { get; set; } = new();
        public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
        public List<TimeBasedReportDto> TimeBasedData { get; set; } = new();
        public List<TopExpenseDto> TopExpenses { get; set; } = new();
    }
}