namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ReportRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Username { get; set; } // For admin use
        public string? Category { get; set; } // Optional filter
        public int TopExpensesLimit { get; set; } = 10;
    }
}