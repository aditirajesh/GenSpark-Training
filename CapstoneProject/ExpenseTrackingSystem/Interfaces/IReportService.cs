using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IReportService
    {
        Task<ReportSummaryDto> GetQuickSummaryAsync(string requestingUser, string? targetUsername = null, int lastNDays = 30);
        Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(string requestingUser, DateTime startDate, DateTime endDate, string? targetUsername = null);
        Task<List<TimeBasedReportDto>> GetTimeBasedReportAsync(string requestingUser, DateTime startDate, DateTime endDate, string? targetUsername = null, string groupBy = "month");
        Task<List<TopExpenseDto>> GetTopExpensesAsync(string requestingUser, DateTime startDate, DateTime endDate, string? targetUsername = null, int limit = 10);
        Task<DetailedReportDto> GetDetailedReportAsync(string requestingUser, ReportRequestDto request);
    }
}