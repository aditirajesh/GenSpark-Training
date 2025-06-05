using FileHandlerApplication.Models;
using FileHandlerApplication.Models.DTOs;

namespace FileHandlerApplication.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUser(UserAddRequestDto dto);
        Task<UserLoginResponseDto> Login(UserLoginRequestDto dto);
    }
}