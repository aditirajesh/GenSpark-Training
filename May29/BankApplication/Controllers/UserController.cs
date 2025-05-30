using BankApplication.Interfaces;
using BankApplication.Models;
using BankApplication.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankApplication.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userservice;
        public UserController(IUserService userService)
        {
            _userservice = userService;
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] UserAddRequestDto dto)
        {
            try
            {
                var result = await _userservice.CreateUser(dto);
                if (result != null)
                {
                    return Created("", result);
                }
                return BadRequest("Unable to process request at the moment");

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("accounts-from-user/{user_id}")]
        public async Task<ActionResult<ICollection<Account>>> GetUserAccounts(int user_id)
        {
            try
            {
                var result = await _userservice.GetAccounts(user_id);
                if (result == null)
                {
                    return BadRequest("Unable to find accounts for this user.");
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpGet("user-by-name/{name}")]
        public async Task<ActionResult<User>> GetUser(string name)
        {
            try
            {
                var result = await _userservice.GetUserByName(name);
                if (result == null)
                {
                    return BadRequest("Unable to find user.");
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}