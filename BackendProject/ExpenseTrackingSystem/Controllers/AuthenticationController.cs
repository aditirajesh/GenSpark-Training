
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginResponseDto>> Login([FromBody] UserLoginRequestDto dto)
        {
            try
            {
                var response = await _authService.Login(dto);
                if (response == null)
                {
                    return Unauthorized("Invalid username or password.");
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest($"Login failed: {e.Message}");
            }
        
        }

        [HttpPost("getaccesstoken")]
        public async Task<ActionResult<AuthResponseDto>> GetAccessToken([FromBody] TokenRefreshRequestDto dto)
        {
            try
            {
               
                var response = await _authService.GetAccessToken(dto.Username, dto.RefreshToken);
                if (response == null)
                {
                    return BadRequest("Unable to refresh token, please log in again.");
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest($"Token refresh failed: {e.Message}");
            }
        }
    }
}