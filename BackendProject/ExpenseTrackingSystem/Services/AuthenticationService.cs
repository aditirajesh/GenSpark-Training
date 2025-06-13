

using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<string, User> _userRepository;
        private readonly ITokenService _tokenservice;

        public AuthenticationService(IEncryptionService encryptionService,
                                IRepository<string, User> userRepository,
                                ITokenService tokenService)
        {
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _tokenservice = tokenService;                          
        }

        public async Task<AuthResponseDto> GetAccessToken(string username, string input_refreshtoken)
        {
            var current_user = await _userRepository.GetByID(username);
            if (current_user == null)
            {
                throw new EntityNotFoundException("Could not find User");
            }

            if (input_refreshtoken != current_user.RefreshToken|| current_user.RefreshTokenExpiryTime < DateTimeOffset.UtcNow)
            {
                throw new SecurityTokenException("Invalid or expired refresh token");
            }

            var access_token = await _tokenservice.GenerateToken(current_user);
            var refresh_token = _tokenservice.GenerateRefreshToken();

            current_user.RefreshToken = refresh_token;
            current_user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
            await _userRepository.Update(username, current_user);

            return new AuthResponseDto
            {
                AccessToken = access_token,
                RefreshToken = refresh_token
            };
        }

        public async Task<UserLoginResponseDto> Login(UserLoginRequestDto dto)
        {
            var dbUser = await _userRepository.GetByID(dto.Username);
            if (dbUser == null)
            {
                throw new EntityNotFoundException("User not found");
            }

            if (!_encryptionService.VerifyData(dto.Password, dbUser.Password))
            {
                throw new InvalidPasswordException("Invalid password");
            }

            var token = await _tokenservice.GenerateToken(dbUser);
            dbUser.RefreshToken = _tokenservice.GenerateRefreshToken();
            dbUser.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
            await _userRepository.Update(dbUser.Username, dbUser);
            return new UserLoginResponseDto()
            {
                Username = dbUser.Username,
                AccessToken = token,
                RefreshToken = dbUser.RefreshToken
            };

        }
    }
}