using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IUserService
    {
        Task<ICollection<User>> GetAllUsers();
        Task<User> GetUserByUsername(string username);
        Task<User> CreateUser(UserAddRequestDto dto);
        Task<User> UpdateUserDetails(string username, UserUpdateRequestDto dto, bool isAdmin, bool isUpdatingSelf);
        Task<User> DeleteUser(string username,string deletedBy);
        Task<ICollection<User>> SearchUsers(UserSearchModel searchModel);

    }
}