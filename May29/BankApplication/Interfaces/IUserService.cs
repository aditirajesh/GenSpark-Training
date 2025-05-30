using BankApplication.Models;
using BankApplication.Models.DTOs;

namespace BankApplication.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUser(UserAddRequestDto dto);
        Task<ICollection<Account>> GetAccounts(int user_id);
        Task<User> GetUserByName(string name);

    }
}