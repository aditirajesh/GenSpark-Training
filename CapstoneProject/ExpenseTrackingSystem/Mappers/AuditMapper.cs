using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Mappers
{
    public class AuditMapper
    {
        public AuditLog MapAddRequestAuditLog(AuditAddRequestDto dto)
        {
            var audit = new AuditLog();
            audit.Action = dto.Action;
            audit.EntityName = dto.EntityName;
            audit.Details = dto.Details;
            audit.Timestamp = dto.Timestamp;
            audit.Username = dto.Username;

            return audit;
        }
    }
}