namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ReportSummaryDto
    {
        public string ReportType { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Timeline { get; set; } = string.Empty; // e.g., "2025-01 to 2025-03"
        public decimal TotalExpense { get; set; }
        public int TotalExpenseCount { get; set; }
        public decimal AverageExpenseAmount { get; set; }
        public string TopCategory { get; set; } = string.Empty;
        public decimal TopCategoryAmount { get; set; }
        public string Period { get; set; } = string.Empty; // e.g., "Q1 2025", "March 2025"
    }
}