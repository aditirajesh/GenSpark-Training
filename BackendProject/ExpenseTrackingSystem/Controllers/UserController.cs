using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] UserAddRequestDto dto)
        {
            try
            {
                var user = await _userService.CreateUser(dto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<ICollection<User>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("search")]
        public async Task<ActionResult<ICollection<User>>> SearchUsers([FromBody] UserSearchModel search)
        {
            try
            {
                var result = await _userService.SearchUsers(search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get/{username}")]
        public async Task<ActionResult<User>> GetUser(string username)
        {
            try
            {
                if (!User.IsInRole("Admin") && User.Identity?.Name != username)
                    return Unauthorized("You are not authorized to access this user's data.");

                var user = await _userService.GetUserByUsername(username);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{username}")]
        public async Task<ActionResult<User>> UpdateUser(string username, [FromBody] UserUpdateRequestDto dto)
        {
            try
            {
                var currentUserName = User.Identity?.Name;
                var isAdmin = User.IsInRole("Admin");
                var isUpdatingSelf = currentUserName == username;

                // Authorization check
                if (!isAdmin && !isUpdatingSelf)
                    return Unauthorized("You are not authorized to update this user's data.");

                // Role change validation
                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    if (!isAdmin)
                        return Unauthorized("Only administrators can change user roles.");
                    
                    if (isUpdatingSelf)
                        return BadRequest("Administrators cannot change their own role.");
                }

                var updated = await _userService.UpdateUserDetails(username, dto, isAdmin, isUpdatingSelf);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{username}")]
        public async Task<ActionResult<User>> DeleteUser(string username)
        {
            try
            {
                var deletedBy = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(deletedBy))
                    return Unauthorized("You must be logged in to delete a user.");
                var deleted = await _userService.DeleteUser(username,deletedBy);
                return Ok(deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
