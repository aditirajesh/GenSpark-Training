public class AuditLog
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;  
    public string EntityName { get; set; } = string.Empty; //can be entity or user
    public Guid? EntityId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? Details { get; set; } 
}
