using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IAuditLogService
    {
        Task<AuditLog> LogAction(AuditAddRequestDto dto);
        Task<ICollection<AuditLog>> GetAllAuditLogs();
    }
}