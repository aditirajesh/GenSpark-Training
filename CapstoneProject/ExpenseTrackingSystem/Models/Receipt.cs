namespace ExpenseTrackingSystem.Models
{
    public class Receipt
    {
        public Guid Id { get; set; }
        public string ReceiptName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public string FilePath { get; set; } = string.Empty;
        public Guid ExpenseId { get; set; }
        public string Username { get; set; } = string.Empty;
        public Expense? Expense { get; set; }
        public User? User { get; set; }
    }
}