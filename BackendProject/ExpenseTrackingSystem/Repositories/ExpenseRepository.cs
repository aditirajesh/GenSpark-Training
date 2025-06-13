
using ExpenseTrackingSystem.Contexts;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackingSystem.Repositories
{
    public class ExpenseRepository : Repository<Guid, Expense>
    {
        public ExpenseRepository(ExpenseContext context): base(context)
        {
            
        }
        public override async Task<Expense> GetByID(Guid key)
        {
            return await _expenseContext.Expenses
                                        .Include(e => e.User)
                                        .Include(e => e.Receipt)
                                        .SingleOrDefaultAsync(u => u.Id == key);
        }

        public override async Task<IEnumerable<Expense>> GetAll()
        {
            return await _expenseContext.Expenses
                                        .Where(e => e.Username != null) // Only get expenses with valid users
                                        .ToListAsync();
        }
            
    }
}