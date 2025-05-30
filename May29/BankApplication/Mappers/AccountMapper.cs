using BankApplication.Models;
using BankApplication.Models.DTOs;

namespace BankApplication.Mappers
{
    public class AccountMapper
    {
        public Account? MapAddRequestAccount(AccountAddRequestDto dto)
        {
            Account account = new();
            account.UserId = dto.UserId;
            account.Type = dto.Type;
            account.Balance = dto.Balance;
            return account;
        }
    }
}