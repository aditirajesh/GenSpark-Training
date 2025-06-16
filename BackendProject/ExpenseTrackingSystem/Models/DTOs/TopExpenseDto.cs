namespace ExpenseTrackingSystem.Models.DTOs
{
    public class TopExpenseDto
    {
        public Guid ExpenseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool HasReceipt { get; set; }
    }

}