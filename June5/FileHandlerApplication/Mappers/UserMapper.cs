using FileHandlerApplication.Models;
using FileHandlerApplication.Models.DTOs;

namespace FileHandlerApplication.Mappers
{
    public class UserMapper
    {
        public User MapRequestToUser(UserAddRequestDto dto)
        {
            User user = new();
            user.Username = dto.Username;
            user.Role = dto.Role;
            return user;
        }
    }
}