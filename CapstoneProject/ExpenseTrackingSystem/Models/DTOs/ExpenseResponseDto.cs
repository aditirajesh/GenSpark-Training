namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ExpenseResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty; 
        public string? UpdatedBy { get; set; } 
        public string Username { get; set; } = string.Empty;
        
        public ReceiptMetadataDto? Receipt { get; set; }
    }
}