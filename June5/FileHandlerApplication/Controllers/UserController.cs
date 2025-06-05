using FileHandlerApplication.Interfaces;
using FileHandlerApplication.Models;
using FileHandlerApplication.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FileHandlerApplication.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginResponseDto>> UserLogin(UserLoginRequestDto loginRequest)
        {
            try
            {
                var result = await _userService.Login(loginRequest);
                return Ok(result);
            }
            catch (Exception e)
            {
                return Unauthorized(e.Message);
            }
        }

        [HttpPost("signup")]
        public async Task<ActionResult<User>> UserSignUp([FromBody] UserAddRequestDto dto)
        {
            try
            {
                var result = await _userService.CreateUser(dto);
                if (result == null)
                {
                    return BadRequest("Unable to create user at the moment");
                }
                return Created("", result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}