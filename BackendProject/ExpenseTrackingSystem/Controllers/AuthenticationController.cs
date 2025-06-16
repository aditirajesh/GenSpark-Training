using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging; // Add this using statement

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger; // Add this field

        public AuthController(IAuthenticationService authService, ILogger<AuthController> logger) // Add logger parameter
        {
            _authService = authService;
            _logger = logger; // Assign logger
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginResponseDto>> Login([FromBody] UserLoginRequestDto dto)
        {
            using var scope = _logger.BeginScope("LoginRequest for {Username} from {RemoteIP}", 
                dto?.Username, HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("Login attempt for user {Username} from IP {RemoteIP}", 
                dto?.Username, HttpContext.Connection.RemoteIpAddress?.ToString());

            if (dto == null)
            {
                _logger.LogWarning("Login attempt with null request body from IP {RemoteIP}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest("Invalid request body");
            }

            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Login attempt with missing username or password for user {Username} from IP {RemoteIP}", 
                    dto.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest("Username and password are required");
            }

            try
            {
                _logger.LogDebug("Processing login request for user {Username}", dto.Username);
                var response = await _authService.Login(dto);
                
                if (response == null)
                {
                    _logger.LogWarning("Login failed for user {Username} - service returned null response", dto.Username);
                    return Unauthorized("Invalid username or password.");
                }

                _logger.LogInformation("Login successful for user {Username} from IP {RemoteIP}", 
                    dto.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                
                // Log successful login without exposing sensitive token data
                _logger.LogDebug("Authentication tokens generated successfully for user {Username}", dto.Username);
                
                return Ok(response);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Login failed for user {Username} - user not found: {ErrorMessage}", 
                    dto.Username, ex.Message);
                return Unauthorized("Invalid username or password.");
            }
            catch (ExpenseTrackingSystem.Exceptions.InvalidPasswordException ex)
            {
                _logger.LogWarning("Login failed for user {Username} - invalid password: {ErrorMessage}", 
                    dto.Username, ex.Message);
                return Unauthorized("Invalid username or password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for user {Username} from IP {RemoteIP}", 
                    dto.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest($"Login failed: {ex.Message}");
            }
        }

        [HttpPost("getaccesstoken")]
        public async Task<ActionResult<AuthResponseDto>> GetAccessToken([FromBody] TokenRefreshRequestDto dto)
        {
            using var scope = _logger.BeginScope("TokenRefresh for {Username} from {RemoteIP}", 
                dto?.Username, HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("Token refresh attempt for user {Username} from IP {RemoteIP}", 
                dto?.Username, HttpContext.Connection.RemoteIpAddress?.ToString());

            if (dto == null)
            {
                _logger.LogWarning("Token refresh attempt with null request body from IP {RemoteIP}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest("Invalid request body");
            }

            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                _logger.LogWarning("Token refresh attempt with missing username or refresh token for user {Username} from IP {RemoteIP}", 
                    dto.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest("Username and refresh token are required");
            }

            try
            {
                _logger.LogDebug("Processing token refresh request for user {Username}", dto.Username);
                var response = await _authService.GetAccessToken(dto.Username, dto.RefreshToken);
                
                if (response == null)
                {
                    _logger.LogWarning("Token refresh failed for user {Username} - service returned null response", dto.Username);
                    return BadRequest("Unable to refresh token, please log in again.");
                }

                _logger.LogInformation("Token refresh successful for user {Username} from IP {RemoteIP}", 
                    dto.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                
                _logger.LogDebug("New authentication tokens generated successfully for user {Username}", dto.Username);
                
                return Ok(response);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Token refresh failed for user {Username} - user not found: {ErrorMessage}", 
                    dto.Username, ex.Message);
                return BadRequest("Unable to refresh token, please log in again.");
            }
            catch (ExpenseTrackingSystem.Exceptions.SecurityTokenException ex)
            {
                _logger.LogWarning("Token refresh failed for user {Username} - invalid or expired refresh token: {ErrorMessage}", 
                    dto.Username, ex.Message);
                return BadRequest("Unable to refresh token, please log in again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh for user {Username} from IP {RemoteIP}", 
                    dto.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest($"Token refresh failed: {ex.Message}");
            }
        }
    }
}