using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<Guid, AuditLog> _auditrepository;
        private readonly AuditMapper _auditMapper;

        public AuditLogService(IRepository<Guid, AuditLog> auditRepository)
        {
            _auditrepository = auditRepository;
            _auditMapper = new();
        }

        public async Task<ICollection<AuditLog>> GetAllAuditLogs()
        {
            var audits = await _auditrepository.GetAll();
            if (audits == null || !audits.Any())
            {
                throw new CollectionEmptyException("No audits present in the database");
            }
            return audits.ToList();
        }

        public async Task<AuditLog> LogAction(AuditAddRequestDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException("Audit cannot be null");
            }
            var audit = _auditMapper.MapAddRequestAuditLog(dto);
            try
            {
                audit = await _auditrepository.Add(audit);
            }
            catch (Exception e)
            {
                throw new EntityCreationException($"Audit could not be created{e.Message}");
            }
            return audit;
        }
    }
}