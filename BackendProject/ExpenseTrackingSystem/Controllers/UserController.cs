using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging; // Add this using statement

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger; // Add this field

        public UserController(IUserService userService, ILogger<UserController> logger) // Add logger parameter
        {
            _userService = userService;
            _logger = logger; // Assign logger
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] UserAddRequestDto dto)
        {
            using var scope = _logger.BeginScope("UserRegistration for {Username} with role {Role}", 
                dto?.Username, dto?.Role);

            _logger.LogInformation("User registration attempt for username {Username} with role {Role} from IP {RemoteIP}", 
                dto?.Username, dto?.Role, HttpContext.Connection.RemoteIpAddress?.ToString());

            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("Registration attempt with null request body from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return BadRequest("Invalid request body.");
                }

                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("Registration attempt with missing username or password for {Username} from IP {RemoteIP}", 
                        dto.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                    return BadRequest("Username and password are required.");
                }

                if (string.IsNullOrWhiteSpace(dto.Role))
                {
                    _logger.LogDebug("No role specified for user {Username}, will use default", dto.Username);
                }

                _logger.LogDebug("Processing user registration for {Username}", dto.Username);
                var user = await _userService.CreateUser(dto);
                
                _logger.LogInformation("User registration successful for {Username} with role {Role} from IP {RemoteIP}", 
                    user.Username, user.Role, HttpContext.Connection.RemoteIpAddress?.ToString());
                
                return Ok(user);
            }
            catch (ExpenseTrackingSystem.Exceptions.DuplicateEntityException ex)
            {
                _logger.LogWarning("Registration failed for {Username} - duplicate username: {ErrorMessage}", 
                    dto?.Username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityCreationException ex)
            {
                _logger.LogError("Registration failed for {Username} - creation error: {ErrorMessage}", 
                    dto?.Username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Username} from IP {RemoteIP}", 
                    dto?.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<ICollection<User>>> GetAll()
        {
            var adminUser = User.Identity?.Name;
            using var scope = _logger.BeginScope("GetAllUsers by {AdminUser}", adminUser);

            _logger.LogInformation("Get all users request by admin {AdminUser} from IP {RemoteIP}", 
                adminUser, HttpContext.Connection.RemoteIpAddress?.ToString());

            try
            {
                _logger.LogDebug("Retrieving all users from service");
                var users = await _userService.GetAllUsers();
                
                _logger.LogInformation("Successfully retrieved {UserCount} users for admin {AdminUser}", 
                    users.Count, adminUser);
                
                return Ok(users);
            }
            catch (ExpenseTrackingSystem.Exceptions.CollectionEmptyException ex)
            {
                _logger.LogWarning("Get all users failed for admin {AdminUser} - no users found: {ErrorMessage}", 
                    adminUser, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving all users for admin {AdminUser}", adminUser);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("search")]
        public async Task<ActionResult<ICollection<User>>> SearchUsers([FromBody] UserSearchModel search)
        {
            var adminUser = User.Identity?.Name;
            using var scope = _logger.BeginScope("SearchUsers by {AdminUser}", adminUser);

            _logger.LogInformation("User search request by admin {AdminUser} with criteria: Username={Username}, Role={Role} from IP {RemoteIP}", 
                adminUser, search?.Username, search?.Role, HttpContext.Connection.RemoteIpAddress?.ToString());

            try
            {
                if (search == null)
                {
                    _logger.LogDebug("Search request with null criteria from admin {AdminUser}, using empty search model", adminUser);
                    search = new UserSearchModel();
                }

                _logger.LogDebug("Processing user search for admin {AdminUser}", adminUser);
                var result = await _userService.SearchUsers(search);
                
                _logger.LogInformation("User search completed for admin {AdminUser}, returning {UserCount} users", 
                    adminUser, result.Count);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user search by admin {AdminUser}", adminUser);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get/{username}")]
        public async Task<ActionResult<User>> GetUser(string username)
        {
            var requestingUser = User.Identity?.Name;
            using var scope = _logger.BeginScope("GetUser {TargetUsername} by {RequestingUser}", username, requestingUser);

            _logger.LogInformation("Get user request for {TargetUsername} by {RequestingUser} from IP {RemoteIP}", 
                username, requestingUser, HttpContext.Connection.RemoteIpAddress?.ToString());

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Get user request with empty username from {RequestingUser}", requestingUser);
                    return BadRequest("Username is required.");
                }

                var isAdmin = User.IsInRole("Admin");
                var isAccessingOwnData = requestingUser == username;

                _logger.LogDebug("Authorization check for {RequestingUser}: IsAdmin={IsAdmin}, IsAccessingOwnData={IsAccessingOwnData}", 
                    requestingUser, isAdmin, isAccessingOwnData);

                if (!isAdmin && !isAccessingOwnData)
                {
                    _logger.LogWarning("Unauthorized access attempt - user {RequestingUser} tried to access {TargetUsername}'s data", 
                        requestingUser, username);
                    return Unauthorized("You are not authorized to access this user's data.");
                }

                _logger.LogDebug("Authorization successful, retrieving user {TargetUsername}", username);
                var user = await _userService.GetUserByUsername(username);
                
                _logger.LogInformation("Successfully retrieved user {TargetUsername} for {RequestingUser}", 
                    username, requestingUser);
                
                return Ok(user);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Get user failed - user {TargetUsername} not found for requester {RequestingUser}: {ErrorMessage}", 
                    username, requestingUser, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user {TargetUsername} for {RequestingUser}", 
                    username, requestingUser);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{username}")]
        public async Task<ActionResult<User>> UpdateUser(string username, [FromBody] UserUpdateRequestDto dto)
        {
            var currentUserName = User.Identity?.Name;
            using var scope = _logger.BeginScope("UpdateUser {TargetUsername} by {CurrentUser}", username, currentUserName);

            _logger.LogInformation("User update request for {TargetUsername} by {CurrentUser} from IP {RemoteIP}", 
                username, currentUserName, HttpContext.Connection.RemoteIpAddress?.ToString());

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Update user request with empty username from {CurrentUser}", currentUserName);
                    return BadRequest("Username is required.");
                }

                if (dto == null)
                {
                    _logger.LogWarning("Update user request with null DTO for {TargetUsername} from {CurrentUser}", 
                        username, currentUserName);
                    return BadRequest("Invalid request body.");
                }

                var isAdmin = User.IsInRole("Admin");
                var isUpdatingSelf = currentUserName == username;

                _logger.LogDebug("Authorization check for {CurrentUser} updating {TargetUsername}: IsAdmin={IsAdmin}, IsUpdatingSelf={IsUpdatingSelf}", 
                    currentUserName, username, isAdmin, isUpdatingSelf);

                // Authorization check
                if (!isAdmin && !isUpdatingSelf)
                {
                    _logger.LogWarning("Unauthorized update attempt - user {CurrentUser} tried to update {TargetUsername}", 
                        currentUserName, username);
                    return Unauthorized("You are not authorized to update this user's data.");
                }

                // Role change validation
                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    _logger.LogDebug("Role change requested for {TargetUsername} to role {NewRole} by {CurrentUser}", 
                        username, dto.Role, currentUserName);
                    
                    if (!isAdmin)
                    {
                        _logger.LogWarning("Unauthorized role change attempt - non-admin {CurrentUser} tried to change role for {TargetUsername}", 
                            currentUserName, username);
                        return Unauthorized("Only administrators can change user roles.");
                    }
                    
                    if (isUpdatingSelf)
                    {
                        _logger.LogWarning("Invalid role change attempt - admin {CurrentUser} tried to change own role", currentUserName);
                        return BadRequest("Administrators cannot change their own role.");
                    }

                    _logger.LogInformation("Role change authorized for {TargetUsername} to {NewRole} by admin {CurrentUser}", 
                        username, dto.Role, currentUserName);
                }

                var fieldsToUpdate = new List<string>();
                if (!string.IsNullOrWhiteSpace(dto.Password)) fieldsToUpdate.Add("password");
                if (!string.IsNullOrWhiteSpace(dto.Phone)) fieldsToUpdate.Add("phone");
                if (!string.IsNullOrWhiteSpace(dto.Role)) fieldsToUpdate.Add("role");

                _logger.LogDebug("Updating user {TargetUsername} with fields: {UpdatedFields}", 
                    username, string.Join(", ", fieldsToUpdate));

                var updated = await _userService.UpdateUserDetails(username, dto, isAdmin, isUpdatingSelf);
                
                _logger.LogInformation("Successfully updated user {TargetUsername} by {CurrentUser} with fields: {UpdatedFields}", 
                    username, currentUserName, string.Join(", ", fieldsToUpdate));
                
                return Ok(updated);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Update user failed - user {TargetUsername} not found for requester {CurrentUser}: {ErrorMessage}", 
                    username, currentUserName, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Update user failed - unauthorized operation by {CurrentUser} for {TargetUsername}: {ErrorMessage}", 
                    currentUserName, username, ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Update user failed - invalid operation by {CurrentUser} for {TargetUsername}: {ErrorMessage}", 
                    currentUserName, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user {TargetUsername} by {CurrentUser}", 
                    username, currentUserName);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{username}")]
        public async Task<ActionResult<User>> DeleteUser(string username)
        {
            var deletedBy = User.Identity?.Name;
            using var scope = _logger.BeginScope("DeleteUser {TargetUsername} by {AdminUser}", username, deletedBy);

            _logger.LogInformation("User deletion request for {TargetUsername} by admin {AdminUser} from IP {RemoteIP}", 
                username, deletedBy, HttpContext.Connection.RemoteIpAddress?.ToString());

            try
            {
                if (string.IsNullOrWhiteSpace(deletedBy))
                {
                    _logger.LogWarning("Delete user request failed - no authenticated user from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Unauthorized("You must be logged in to delete a user.");
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Delete user request with empty username from admin {AdminUser}", deletedBy);
                    return BadRequest("Username is required.");
                }

                if (deletedBy.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Self-deletion attempt by admin {AdminUser}", deletedBy);
                    return BadRequest("Administrators cannot delete their own account.");
                }

                _logger.LogDebug("Processing user deletion for {TargetUsername} by admin {AdminUser}", username, deletedBy);
                var deleted = await _userService.DeleteUser(username, deletedBy);
                
                _logger.LogInformation("Successfully deleted user {TargetUsername} by admin {AdminUser}", 
                    username, deletedBy);
                
                return Ok(deleted);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Delete user failed - user {TargetUsername} not found for admin {AdminUser}: {ErrorMessage}", 
                    username, deletedBy, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Delete user failed - invalid operation by admin {AdminUser} for {TargetUsername}: {ErrorMessage}", 
                    deletedBy, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user {TargetUsername} by admin {AdminUser}", 
                    username, deletedBy);
                return BadRequest(ex.Message);
            }
        }
    }
}