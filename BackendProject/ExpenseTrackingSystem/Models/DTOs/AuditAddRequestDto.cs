namespace ExpenseTrackingSystem.Models.DTOs
{
    public class AuditAddRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
        public string? Details { get; set; } 

    }
}