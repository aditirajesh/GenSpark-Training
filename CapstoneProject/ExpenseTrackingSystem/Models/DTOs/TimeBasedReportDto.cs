namespace ExpenseTrackingSystem.Models.DTOs
{
    public class TimeBasedReportDto
    {
        public string TimePeriod { get; set; } = string.Empty; // "2025-01", "Week 1", etc.
        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
        public decimal AverageAmount { get; set; }
        public List<CategoryBreakdownDto> TopCategories { get; set; } = new();
    }
}
