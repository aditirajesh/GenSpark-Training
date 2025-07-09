namespace ExpenseTrackingSystem.Models.DTOs
{

    public class ReceiptMetadataDto
    {
        public Guid Id { get; set; }
        public string ReceiptName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public Guid ExpenseId { get; set; }
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
        
        public string DownloadUrl => $"/api/receipts/download/{Id}";
        public string PreviewUrl => $"/api/receipts/preview/{Id}";
        public string InfoUrl => $"/api/receipts/info/{Id}";
    }
}