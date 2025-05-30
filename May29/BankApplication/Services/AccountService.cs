using BankApplication.Interfaces;
using BankApplication.Mappers;
using BankApplication.Models;
using BankApplication.Models.DTOs;

namespace BankApplication.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<int, Account> _accountrepository;
        private readonly IRepository<int, User> _userrepository;
        AccountMapper accountMapper;

        public AccountService(IRepository<int, Account> accountrepository,
                            IRepository<int, User> userrepository,
                            AccountMapper accountmapper)
        {
            _accountrepository = accountrepository;
            _userrepository = userrepository;
            accountMapper = accountmapper;
        }

        public async Task<Account> CreateAccount(AccountAddRequestDto dto)
        {
            var user = await _userrepository.GetById(dto.UserId);
            if (user == null)
            {
                throw new Exception("User not found, cannot create account");
            }

            var new_account = accountMapper.MapAddRequestAccount(dto);
            new_account = await _accountrepository.Add(new_account);
            if (new_account == null)
            {
                throw new Exception("Could not add account");
            }

            new_account.User = user;
            new_account.CreatedAt = DateTime.UtcNow;

            if (user.Accounts == null)
            {
                user.Accounts = new List<Account>();
            }
            user.Accounts.Add(new_account);
            return new_account;

        }

        public async Task<User> GetUserFromAccount(int account_id)
        {
            var account = await _accountrepository.GetById(account_id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }
            var user = await _userrepository.GetById(account.UserId);
            if (user == null)
            {
                throw new Exception("User not found for this account.");
            }

            return user;
        }

        public async Task<decimal> GetAccountBalance(int account_id)
        {
            var account = await _accountrepository.GetById(account_id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }
            return account.Balance;
        }
    }
}