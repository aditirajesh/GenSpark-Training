using System.Collections;
using BankApplication.Contexts;
using BankApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Repositories
{
    public class UserRepository : Repository<int, User>
    {
        public UserRepository(BankingContext bankingContext) : base(bankingContext) { }
        public override async Task<User> GetById(int id)
        {
            var user = await _bankingcontext.Users
                                                .Include(u => u.Accounts) // load accounts
                                                .FirstOrDefaultAsync(u => u.UserId == id);
            if (user != null)
            {
                return user;
            }
            throw new Exception("User not found.");

        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            var users = _bankingcontext.Users;
            if (!users.Any() || users != null)
            {
                return await users.ToListAsync();
            }
            throw new Exception("No users found.");
        }
    }
}