namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ReceiptInfoDto
    {
        public Guid Id { get; set; }
        public string ReceiptName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }
    }
    
}
