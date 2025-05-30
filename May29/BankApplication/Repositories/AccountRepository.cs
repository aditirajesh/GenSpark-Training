using System.Collections;
using BankApplication.Contexts;
using BankApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Repositories
{
    public class AccountRepository : Repository<int, Account>
    {
        public AccountRepository(BankingContext bankingContext) : base(bankingContext) { }
        public override async Task<Account> GetById(int id)
        {
            var account = await _bankingcontext.Accounts
                                                        .Include(a => a.SentBankTransactions)
                                                        .Include(a => a.ReceivedBankTransactions)
                                                        .Include(u => u.User)
                                                        .FirstOrDefaultAsync(u => u.AccountId == id);
            if (account != null)
            {
                return account;
            }
            throw new Exception("account not found.");

        }

        public override async Task<IEnumerable<Account>> GetAll()
        {
            var accounts = _bankingcontext.Accounts;
            if (!accounts.Any() || accounts != null)
            {
                return await accounts.ToListAsync();
            }
            throw new Exception("No accounts found.");
        }
    }
}