namespace ExpenseTrackingSystem.Models.DTOs
{
    public class QuickSummaryRequestDto
    {
        public string? Username { get; set; } // For admin use
        public int LastNDays { get; set; } = 30; // Default to last 30 days
    }
}