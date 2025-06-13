using ExpenseTrackingSystem.Contexts;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTrackingSystem.Repositories
{
    public class AuditRepository : Repository<Guid, AuditLog>
    {
        public AuditRepository(ExpenseContext ExpenseContext) : base(ExpenseContext)
        {
        }

        public override async Task<IEnumerable<AuditLog>> GetAll()
        {
            return await _expenseContext.AuditLogs.ToListAsync();
        }

        public override async Task<AuditLog> GetByID(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}