using BankApplication.Models;
using BankApplication.Models.DTOs;

namespace BankApplication.Interfaces
{
    public interface IAccountService
    {
        Task<Account> CreateAccount(AccountAddRequestDto dto);
        Task<User> GetUserFromAccount(int account_id);
        Task<decimal> GetAccountBalance(int accountId);

    }
}