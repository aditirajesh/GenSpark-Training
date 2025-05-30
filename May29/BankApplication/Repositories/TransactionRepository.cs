using System.Collections;
using BankApplication.Contexts;
using BankApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Repositories
{
    public class BankTransactionRepository : Repository<int, BankTransaction>
    {
        public BankTransactionRepository(BankingContext bankingContext) : base(bankingContext) { }
        public override async Task<BankTransaction> GetById(int id)
        {
            var BankTransaction = await _bankingcontext.BankTransactions.SingleOrDefaultAsync(u => u.BankTransactionId == id);
            if (BankTransaction != null)
            {
                return BankTransaction;
            }
            throw new Exception("BankTransaction not found.");

        }

        public override async Task<IEnumerable<BankTransaction>> GetAll()
        {
            var BankTransactions = _bankingcontext.BankTransactions;
            if (!BankTransactions.Any() || BankTransactions != null)
            {
                return await BankTransactions.ToListAsync();
            }
            throw new Exception("No BankTransactions found.");
        }
    }
}