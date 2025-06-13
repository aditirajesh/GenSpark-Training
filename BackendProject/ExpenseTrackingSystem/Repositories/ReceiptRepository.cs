
using ExpenseTrackingSystem.Contexts;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackingSystem.Repositories
{
    public class ReceiptRepository : Repository<Guid,Receipt>
    {
        public ReceiptRepository(ExpenseContext context): base(context)
        {
            
        }

        
        public override async Task<Receipt> GetByID(Guid key)
        {
            return await _expenseContext.Receipts
                                        .Include(r => r.Expense)
                                        .Include(r => r.User)
                                        .SingleOrDefaultAsync(u => u.Id == key);
        }

        public override async Task<IEnumerable<Receipt>> GetAll()
        {
            return await _expenseContext.Receipts
                                        .Where(r => r.Username != null) // Only get receipts with valid users
                                        .ToListAsync();
        }
            
    }
}