using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Misc;
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
    [Route("api/expenses")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly ILogger<ExpenseController> _logger; // Add this field

        public ExpenseController(IExpenseService expenseService,
                                IUserService userService,
                                ILogger<ExpenseController> logger) // Add logger parameter
        {
            _expenseService = expenseService;
            _userService = userService;
            _logger = logger; // Assign logger
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<Expense>> AddExpense([FromForm] ExpenseAddRequestDto dto)
        {
            var currentUsername = User.Identity?.Name;
            using var scope = _logger.BeginScope("AddExpense by {CurrentUser} for {TargetUser} amount {Amount}", 
                currentUsername, dto?.TargetUsername ?? currentUsername, dto?.Amount);

            _logger.LogInformation("Add expense request from user {CurrentUsername} for target {TargetUsername} with amount {Amount} and category {Category}", 
                currentUsername, dto?.TargetUsername, dto?.Amount, dto?.Category);

            try
            {
                if (string.IsNullOrWhiteSpace(currentUsername))
                {
                    _logger.LogWarning("Add expense request failed - username not found in token from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Unauthorized("Username not found in token.");
                }

                if (dto == null)
                {
                    _logger.LogWarning("Add expense request failed - null DTO from user {CurrentUsername}", currentUsername);
                    return BadRequest("Invalid expense data provided.");
                }

                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {CurrentUsername} has admin role: {IsAdmin}", currentUsername, isAdmin);
                
                // Determine target username
                string targetUsername;
                
                if (!string.IsNullOrWhiteSpace(dto.TargetUsername))
                {
                    _logger.LogDebug("Admin operation requested - adding expense for {TargetUsername} by {CurrentUsername}", 
                        dto.TargetUsername, currentUsername);
                    
                    // Admin trying to add expense for another user
                    if (!isAdmin)
                    {
                        _logger.LogWarning("Unauthorized admin operation attempt - user {CurrentUsername} tried to add expense for {TargetUsername}", 
                            currentUsername, dto.TargetUsername);
                        return Unauthorized("Only administrators can add expenses for other users.");
                    }
                    
                    targetUsername = dto.TargetUsername;
                    _logger.LogInformation("Admin {CurrentUsername} adding expense for user {TargetUsername}", 
                        currentUsername, targetUsername);
                }
                else
                {
                    // Default: add expense for current user
                    targetUsername = currentUsername;
                    _logger.LogDebug("Adding expense for current user {TargetUsername}", targetUsername);
                }

                // Pass both target username and current username for audit tracking
                _logger.LogDebug("Calling expense service to add expense");
                var result = await _expenseService.AddExpense(dto, targetUsername, currentUsername);
                
                _logger.LogInformation("Successfully added expense {ExpenseId} for user {TargetUsername} with amount {Amount} by {CurrentUsername}", 
                    result.Id, targetUsername, result.Amount, currentUsername);
                
                return Ok(result);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Add expense failed - entity not found for user {CurrentUsername}: {ErrorMessage}", 
                    currentUsername, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityCreationException ex)
            {
                _logger.LogError("Add expense failed - creation error for user {CurrentUsername}: {ErrorMessage}", 
                    currentUsername, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding expense for user {CurrentUsername} targeting {TargetUsername}", 
                    currentUsername, dto?.TargetUsername);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Expense>> UpdateExpense([FromForm] ExpenseUpdateRequestDto dto)
        {
            var username = User.Identity?.Name;
            using var scope = _logger.BeginScope("UpdateExpense {ExpenseId} by {Username}", dto?.Id, username);

            _logger.LogInformation("Update expense request for expense {ExpenseId} by user {Username}", dto?.Id, username);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Update expense request failed - username not found in token from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Unauthorized("Username not found in token.");
                }

                if (dto == null)
                {
                    _logger.LogWarning("Update expense request failed - null DTO from user {Username}", username);
                    return BadRequest("Invalid expense data provided.");
                }

                _logger.LogDebug("Retrieving expense {ExpenseId} for authorization check", dto.Id);
                var expense = await _expenseService.GetExpense(dto.Id);
                
                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {Username} has admin role: {IsAdmin}, expense owner: {ExpenseOwner}", 
                    username, isAdmin, expense.Username);

                if (expense.Username != username && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized update attempt - user {Username} tried to update expense {ExpenseId} owned by {ExpenseOwner}", 
                        username, dto.Id, expense.Username);
                    return Unauthorized("You can only update your own expenses.");
                }

                _logger.LogDebug("Authorization successful, proceeding with expense update");
                var result = await _expenseService.UpdateExpense(dto, username);
                
                _logger.LogInformation("Successfully updated expense {ExpenseId} by user {Username}, new amount: {Amount}", 
                    result.Id, username, result.Amount);
                
                return Ok(result);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Update expense failed - expense {ExpenseId} not found for user {Username}: {ErrorMessage}", 
                    dto?.Id, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityUpdateException ex)
            {
                _logger.LogError("Update expense failed - update error for expense {ExpenseId} by user {Username}: {ErrorMessage}", 
                    dto?.Id, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating expense {ExpenseId} by user {Username}", dto?.Id, username);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<Expense>> DeleteExpense(Guid id)
        {
            var username = User.Identity?.Name;
            using var scope = _logger.BeginScope("DeleteExpense {ExpenseId} by {Username}", id, username);

            _logger.LogInformation("Delete expense request for expense {ExpenseId} by user {Username}", id, username);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Delete expense request failed - username not found in token from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Unauthorized("Username not found in token.");
                }

                _logger.LogDebug("Retrieving expense {ExpenseId} for authorization check", id);
                var expense = await _expenseService.GetExpense(id);
                
                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {Username} has admin role: {IsAdmin}, expense owner: {ExpenseOwner}", 
                    username, isAdmin, expense.Username);

                if (expense.Username != username && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized delete attempt - user {Username} tried to delete expense {ExpenseId} owned by {ExpenseOwner}", 
                        username, id, expense.Username);
                    return Unauthorized("You can only delete your own expenses.");
                }

                _logger.LogInformation("Authorized deletion of expense {ExpenseId} '{ExpenseTitle}' (Amount: {Amount}) owned by {ExpenseOwner} deleted by {Username}", 
                    id, expense.Title, expense.Amount, expense.Username, username);

                // Pass the current username to the service
                var result = await _expenseService.DeleteExpense(id, username);
                
                _logger.LogInformation("Successfully deleted expense {ExpenseId} by user {Username}", id, username);
                return Ok(result);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Delete expense failed - expense {ExpenseId} not found for user {Username}: {ErrorMessage}", 
                    id, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting expense {ExpenseId} by user {Username}", id, username);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get/{id}")]
        public async Task<ActionResult<Expense>> GetExpense(Guid id)
        {
            var username = User.Identity?.Name;
            using var scope = _logger.BeginScope("GetExpense {ExpenseId} by {Username}", id, username);

            _logger.LogDebug("Get expense request for expense {ExpenseId} by user {Username}", id, username);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Get expense request failed - username not found in token from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Unauthorized("Username not found in token.");
                }

                _logger.LogDebug("Retrieving expense {ExpenseId}", id);
                var expense = await _expenseService.GetExpense(id);
                
                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {Username} has admin role: {IsAdmin}, expense owner: {ExpenseOwner}", 
                    username, isAdmin, expense.Username);

                if (expense.Username != username && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized access attempt - user {Username} tried to view expense {ExpenseId} owned by {ExpenseOwner}", 
                        username, id, expense.Username);
                    return Unauthorized("You can only view your own expenses.");
                }

                _logger.LogDebug("Successfully retrieved expense {ExpenseId} '{ExpenseTitle}' for user {Username}", 
                    id, expense.Title, username);

                return Ok(expense);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Get expense failed - expense {ExpenseId} not found for user {Username}: {ErrorMessage}", 
                    id, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving expense {ExpenseId} by user {Username}", id, username);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("search")]
        public async Task<ActionResult<ICollection<Expense>>> SearchExpenses([FromBody] ExpenseSearchModel search)
        {
            var username = User.Identity?.Name;
            using var scope = _logger.BeginScope("SearchExpenses by {Username}", username);

            _logger.LogInformation("Search expenses request by user {Username} with criteria: Title={Title}, Category={Category}", 
                username, search?.Title, search?.Category);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Search expenses request failed - username not found in token from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Unauthorized("Username not found in token.");
                }

                if (search == null)
                {
                    _logger.LogDebug("Search request with null criteria from user {Username}, using empty search model", username);
                    search = new ExpenseSearchModel();
                }

                _logger.LogDebug("Executing expense search for user {Username}", username);
                var results = await _expenseService.SearchExpense(search);
                
                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {Username} has admin role: {IsAdmin}, total results before filtering: {TotalResults}", 
                    username, isAdmin, results.Count);

                if (!isAdmin)
                {
                    var originalCount = results.Count;
                    results = results.Where(e => e.Username == username).ToList();
                    _logger.LogDebug("Filtered results for non-admin user {Username}: {FilteredResults} out of {OriginalResults}", 
                        username, results.Count, originalCount);
                }

                _logger.LogInformation("Search completed for user {Username}, returning {ResultCount} expenses", 
                    username, results.Count);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during expense search by user {Username}", username);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<ICollection<Expense>>> GetAllExpenses(
            [FromQuery] string? username,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var requester = User.Identity?.Name;
            using var scope = _logger.BeginScope("GetAllExpenses by {Requester} for {TargetUser} page {PageNumber}", 
                requester, username ?? requester, pageNumber);

            _logger.LogInformation("Get all expenses request by {Requester} for user {TargetUsername} (Page: {PageNumber}, Size: {PageSize})", 
                requester, username, pageNumber, pageSize);

            try
            {
                if (string.IsNullOrWhiteSpace(requester))
                {
                    _logger.LogWarning("Get all expenses request failed - username not found in token from IP {RemoteIP}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Unauthorized("Username not found in token.");
                }

                if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                {
                    _logger.LogWarning("Invalid pagination parameters from user {Requester}: PageNumber={PageNumber}, PageSize={PageSize}", 
                        requester, pageNumber, pageSize);
                    return BadRequest("Invalid pagination parameters. Page number must be >= 1 and page size must be between 1 and 100.");
                }

                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {Requester} has admin role: {IsAdmin}", requester, isAdmin);

                if (!string.IsNullOrWhiteSpace(username) && username != requester && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized access attempt - user {Requester} tried to access expenses for {TargetUsername}", 
                        requester, username);
                    return Unauthorized("You are not authorized to access other users' expenses.");
                }

                var userToQuery = string.IsNullOrWhiteSpace(username) ? requester : username;
                _logger.LogDebug("Querying expenses for user {UserToQuery}", userToQuery);

                var results = await _expenseService.SearchExpense(new ExpenseSearchModel());
                var totalCount = results.Where(e => e.Username == userToQuery).Count();
                
                var filtered = results
                                .Where(e => e.Username == userToQuery)
                                .OrderByDescending(e => e.CreatedAt)
                                .Paginate(pageNumber, pageSize)
                                .ToList();

                _logger.LogInformation("Retrieved {ReturnedCount} expenses out of {TotalCount} for user {UserToQuery} (Page {PageNumber})", 
                    filtered.Count, totalCount, userToQuery, pageNumber);

                return Ok(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving all expenses for requester {Requester} targeting {TargetUsername}", 
                    requester, username);
                return BadRequest(ex.Message);
            }
        }
    }
}