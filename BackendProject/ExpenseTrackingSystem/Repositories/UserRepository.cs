
using ExpenseTrackingSystem.Contexts;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackingSystem.Repositories
{
    public class UserRepository : Repository<string, User>
    {
        public UserRepository(ExpenseContext context): base(context)
        {
            
        }
        public override async Task<User> GetByID(string key)
        {
            return await _expenseContext.Users
                                        .Include(u => u.Expenses)
                                        .ThenInclude(e => e.Receipt)
                                        .Include(u => u.Receipts)
                                        .SingleOrDefaultAsync(u => u.Username == key);
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            return await _expenseContext.Users
                                        .Where(u => !u.IsDeleted)
                                        .ToListAsync();
        }
            
    }
}