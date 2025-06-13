namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ReceiptResponseDto
    {
        public byte[] fileData { get; set; }
        public string ReceiptName { get; set; } = string.Empty;
        public Guid ExpenseId { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;

    }
}