using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.Extensions.Logging; // Add this using statement

namespace ExpenseTrackingSystem.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<string, User> _userRepository;
        private readonly ITokenService _tokenservice;
        private readonly ILogger<AuthenticationService> _logger; // Add this field

        public AuthenticationService(IEncryptionService encryptionService,
                                IRepository<string, User> userRepository,
                                ITokenService tokenService,
                                ILogger<AuthenticationService> logger) // Add logger parameter
        {
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _tokenservice = tokenService;
            _logger = logger; // Assign logger
        }

        public async Task<AuthResponseDto> GetAccessToken(string username, string input_refreshtoken)
        {
            using var scope = _logger.BeginScope("TokenRefresh for {Username}", username);
            
            _logger.LogInformation("Starting access token refresh for user {Username}", username);

            _logger.LogDebug("Looking up user {Username} for token refresh", username);
            var current_user = await _userRepository.GetByID(username);
            if (current_user == null)
            {
                _logger.LogWarning("User {Username} not found during token refresh attempt", username);
                throw new EntityNotFoundException("Could not find User");
            }

            _logger.LogDebug("Found user {Username}, validating refresh token", username);

            // Log refresh token validation (without exposing actual tokens)
            var isTokenValid = input_refreshtoken == current_user.RefreshToken;
            var isTokenNotExpired = current_user.RefreshTokenExpiryTime >= DateTimeOffset.UtcNow;
            
            _logger.LogDebug("Token validation for user {Username}: TokenMatch={TokenMatch}, NotExpired={NotExpired}, ExpiryTime={ExpiryTime}", 
                username, isTokenValid, isTokenNotExpired, current_user.RefreshTokenExpiryTime);

            if (!isTokenValid || !isTokenNotExpired)
            {
                if (!isTokenValid)
                {
                    _logger.LogWarning("Invalid refresh token provided for user {Username}", username);
                }
                else
                {
                    _logger.LogWarning("Expired refresh token used for user {Username}, expired at {ExpiryTime}", 
                        username, current_user.RefreshTokenExpiryTime);
                }
                throw new SecurityTokenException("Invalid or expired refresh token");
            }

            _logger.LogDebug("Refresh token validated successfully for user {Username}, generating new tokens", username);

            var access_token = await _tokenservice.GenerateToken(current_user);
            var refresh_token = _tokenservice.GenerateRefreshToken();

            _logger.LogDebug("Generated new tokens for user {Username}, updating user record", username);

            current_user.RefreshToken = refresh_token;
            current_user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
            await _userRepository.Update(username, current_user);

            _logger.LogInformation("Successfully refreshed tokens for user {Username}, new expiry: {NewExpiryTime}", 
                username, current_user.RefreshTokenExpiryTime);

            return new AuthResponseDto
            {
                AccessToken = access_token,
                RefreshToken = refresh_token
            };
        }

        public async Task<UserLoginResponseDto> Login(UserLoginRequestDto dto)
        {
            using var scope = _logger.BeginScope("UserLogin for {Username}", dto.Username);
            
            _logger.LogInformation("Starting login attempt for user {Username}", dto.Username);

            _logger.LogDebug("Looking up user {Username} in database", dto.Username);
            var dbUser = await _userRepository.GetByID(dto.Username);
            if (dbUser == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found", dto.Username);
                throw new EntityNotFoundException("User not found");
            }

            _logger.LogDebug("Found user {Username}, verifying password", dto.Username);

            // Verify password without logging the actual password
            bool passwordValid;
            try
            {
                passwordValid = _encryptionService.VerifyData(dto.Password, dbUser.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password verification for user {Username}", dto.Username);
                throw new InvalidPasswordException("Password verification failed");
            }

            if (!passwordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for user {Username}", dto.Username);
                throw new InvalidPasswordException("Invalid password");
            }

            _logger.LogDebug("Password verified successfully for user {Username}, generating authentication tokens", dto.Username);

            string token;
            string refreshToken;
            try
            {
                token = await _tokenservice.GenerateToken(dbUser);
                refreshToken = _tokenservice.GenerateRefreshToken();
                _logger.LogDebug("Successfully generated tokens for user {Username}", dto.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tokens for user {Username}", dto.Username);
                throw new SecurityTokenException("Token generation failed");
            }

            _logger.LogDebug("Updating user {Username} with new refresh token", dto.Username);
            dbUser.RefreshToken = refreshToken;
            dbUser.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
            
            try
            {
                await _userRepository.Update(dbUser.Username, dbUser);
                _logger.LogDebug("Successfully updated refresh token for user {Username}", dto.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refresh token for user {Username}", dto.Username);
                throw new EntityUpdateException("Failed to update user with refresh token");
            }

            _logger.LogInformation("Login successful for user {Username} with role {UserRole}, token expires: {TokenExpiry}", 
                dto.Username, dbUser.Role, dbUser.RefreshTokenExpiryTime);

            return new UserLoginResponseDto()
            {
                Username = dbUser.Username,
                AccessToken = token,
                RefreshToken = dbUser.RefreshToken
            };
        }
    }
}