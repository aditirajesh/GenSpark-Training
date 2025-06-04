using FirstAPI.Models.DTOs;

namespace FirstAPI.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserLoginResponseDto> Login(UserLoginRequestDto dto);
    }
}