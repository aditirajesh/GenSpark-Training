using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Mappers
{
    public class UserMapper
    {
        public User MapAddRequestUser(UserAddRequestDto dto)
        {
            var user = new User();
            user.Username = dto.Username;
            user.Phone = dto.Phone;
            user.Role = dto.Role;
            return user;
        }

        public User MapUpdateRequestUser(UserUpdateRequestDto dto)
        {
            var user = new User();
            user.Phone = dto.Phone;
            user.Role = dto.Role;
            return user;
        }
    }
}