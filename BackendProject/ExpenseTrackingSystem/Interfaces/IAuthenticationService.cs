using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserLoginResponseDto> Login(UserLoginRequestDto dto);
        Task<AuthResponseDto> GetAccessToken(string username, string input_refreshtoken);
    }
}