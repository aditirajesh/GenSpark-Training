using BankApplication.Models;
using BankApplication.Models.DTOs;

namespace BankApplication.Mappers
{
    public class UserMapper
    {
        public User? MapAddRequestUser(UserAddRequestDto dto)
        {
            User user = new();
            user.Name = dto.Name;
            user.Phone = dto.Phone;
            user.Email = dto.Email;
            user.Address = dto.Address;
            return user;
        }
    }
}