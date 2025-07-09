using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Misc;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/expenses")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IExpenseService expenseService,
                                IUserService userService,
                                ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService;
            _userService = userService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<Expense>> AddExpense([FromForm] ExpenseAddRequestDto dto)
        {
            var currentUsername = User.Identity?.Name;
            using var scope = _logger.BeginScope("AddExpense by {CurrentUser} for {TargetUser} amount {Amount} date {ExpenseDate}",
                currentUsername, dto?.TargetUsername ?? currentUsername, dto?.Amount, dto?.ExpenseDate);

            _logger.LogInformation("Add expense request from user {CurrentUsername} for target {TargetUsername} with amount {Amount}, category {Category}, and expense date {ExpenseDate}",
                currentUsername, dto?.TargetUsername, dto?.Amount, dto?.Category, dto?.ExpenseDate?.ToString("yyyy-MM-dd") ?? "current");

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

                // ✅ NEW: Validate expense date on controller level as well (belt and suspenders approach)
                if (dto.ExpenseDate.HasValue && dto.ExpenseDate.Value > DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Add expense request failed - future date {ExpenseDate} provided by user {CurrentUsername}",
                        dto.ExpenseDate.Value, currentUsername);
                    return BadRequest("Expense date cannot be in the future. Please select a past or current date.");
                }

                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {CurrentUsername} has admin role: {IsAdmin}", currentUsername, isAdmin);

                string targetUsername;

                if (!string.IsNullOrWhiteSpace(dto.TargetUsername))
                {
                    _logger.LogDebug("Admin operation requested - adding expense for {TargetUsername} by {CurrentUsername}",
                        dto.TargetUsername, currentUsername);

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
                    targetUsername = currentUsername;
                    _logger.LogDebug("Adding expense for current user {TargetUsername}", targetUsername);
                }

                // ✅ NEW: Log whether custom date or current date is being used
                if (dto.ExpenseDate.HasValue)
                {
                    _logger.LogInformation("Using custom expense date {ExpenseDate} for expense by user {TargetUsername}",
                        dto.ExpenseDate.Value.ToString("yyyy-MM-dd"), targetUsername);
                }
                else
                {
                    _logger.LogDebug("Using current date for expense by user {TargetUsername}", targetUsername);
                }

                _logger.LogDebug("Calling expense service to add expense");
                var result = await _expenseService.AddExpense(dto, targetUsername, currentUsername);

                _logger.LogInformation("Successfully added expense {ExpenseId} for user {TargetUsername} with amount {Amount} on date {ExpenseDate} by {CurrentUsername}",
                    result.Id, targetUsername, result.Amount, result.CreatedAt.ToString("yyyy-MM-dd"), currentUsername);

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
            catch (ArgumentException ex) when (ex.ParamName == "ExpenseDate")
            {
                _logger.LogWarning("Add expense failed - invalid expense date for user {CurrentUsername}: {ErrorMessage}",
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
        public async Task<ActionResult<Expense>> UpdateExpense(Guid id, [FromForm] ExpenseUpdateRequestDto dto)
        {
            var username = User.Identity?.Name;
            using var scope = _logger.BeginScope("UpdateExpense {ExpenseId} by {Username}", id, username);

            _logger.LogInformation("Update expense request for expense {ExpenseId} by user {Username} with new date {ExpenseDate}",
                id, username, dto?.ExpenseDate?.ToString("yyyy-MM-dd") ?? "no change");

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

                // ✅ NEW: Validate expense date on controller level as well
                if (dto.ExpenseDate.HasValue && dto.ExpenseDate.Value > DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Update expense request failed - future date {ExpenseDate} provided by user {Username}",
                        dto.ExpenseDate.Value, username);
                    return BadRequest("Expense date cannot be in the future. Please select a past or current date.");
                }

                // Ensure the DTO has the correct ID from the URL parameter
                dto.Id = id;

                _logger.LogDebug("Retrieving expense {ExpenseId} for authorization check", id);
                var expense = await _expenseService.GetExpense(id);

                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {Username} has admin role: {IsAdmin}, expense owner: {ExpenseOwner}",
                    username, isAdmin, expense.Username);

                if (expense.Username != username && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized update attempt - user {Username} tried to update expense {ExpenseId} owned by {ExpenseOwner}",
                        username, id, expense.Username);
                    return Unauthorized("You can only update your own expenses.");
                }

                // ✅ NEW: Log if expense date is being updated
                if (dto.ExpenseDate.HasValue)
                {
                    _logger.LogInformation("Updating expense date from {OriginalDate} to {NewDate} for expense {ExpenseId}",
                        expense.CreatedAt.ToString("yyyy-MM-dd"), dto.ExpenseDate.Value.ToString("yyyy-MM-dd"), id);
                }

                _logger.LogDebug("Authorization successful, proceeding with expense update");
                var result = await _expenseService.UpdateExpense(dto, username);

                _logger.LogInformation("Successfully updated expense {ExpenseId} by user {Username}, new amount: {Amount}, new date: {ExpenseDate}",
                    result.Id, username, result.Amount, result.CreatedAt.ToString("yyyy-MM-dd"));

                return Ok(result);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityNotFoundException ex)
            {
                _logger.LogWarning("Update expense failed - expense {ExpenseId} not found for user {Username}: {ErrorMessage}",
                    id, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ExpenseTrackingSystem.Exceptions.EntityUpdateException ex)
            {
                _logger.LogError("Update expense failed - update error for expense {ExpenseId} by user {Username}: {ErrorMessage}",
                    id, username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex) when (ex.ParamName == "ExpenseDate")
            {
                _logger.LogWarning("Update expense failed - invalid expense date for user {Username}: {ErrorMessage}",
                    username, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating expense {ExpenseId} by user {Username}", id, username);
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
        public async Task<ActionResult<ExpenseResponseDto>> GetExpense(Guid id, [FromQuery] bool includeReceipt = false)
        {
            var username = User.Identity?.Name;
            using var scope = _logger.BeginScope("GetExpense {ExpenseId} by {Username} (IncludeReceipt: {IncludeReceipt})", 
                id, username, includeReceipt);

            _logger.LogDebug("Get expense request for expense {ExpenseId} by user {Username}, includeReceipt: {IncludeReceipt}", 
                id, username, includeReceipt);

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

                // Create the response DTO
                var response = new ExpenseResponseDto
                {
                    Id = expense.Id,
                    Title = expense.Title,
                    Category = expense.Category,
                    Notes = expense.Notes,
                    Amount = expense.Amount,
                    CreatedAt = expense.CreatedAt,
                    UpdatedAt = expense.UpdatedAt,
                    CreatedBy = expense.CreatedBy, 
                    UpdatedBy = expense.UpdatedBy,
                    Username = expense.Username
                };

                // ✅ FIXED: Include receipt metadata only (no binary data)
                if (includeReceipt && expense.Receipt != null)
                {
                    try
                    {
                        _logger.LogDebug("Including receipt metadata for expense {ExpenseId}, receipt ID: {ReceiptId}", 
                            id, expense.Receipt.Id);
                        
                        // Get receipt metadata without binary data - just get the Receipt entity info
                        response.Receipt = new ReceiptMetadataDto
                        {
                            Id = expense.Receipt.Id,
                            ReceiptName = expense.Receipt.ReceiptName,
                            Category = expense.Receipt.Category,
                            CreatedAt = expense.Receipt.CreatedAt,
                            Username = expense.Receipt.Username ?? expense.Username,
                            ExpenseId = expense.Receipt.ExpenseId,
                            FileSizeBytes = 0, // Will be set when file is accessed
                            ContentType = GetContentType(expense.Receipt.ReceiptName)
                        };
                        
                        _logger.LogInformation("Successfully retrieved expense {ExpenseId} with receipt metadata for user {Username}", 
                            id, username);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retrieve receipt metadata for expense {ExpenseId}, returning expense without receipt", id);
                        // Continue without receipt data - don't fail the entire request
                    }
                }
                else if (includeReceipt && expense.Receipt == null)
                {
                    _logger.LogDebug("Receipt requested but no receipt found for expense {ExpenseId}", id);
                }

                _logger.LogDebug("Successfully retrieved expense {ExpenseId} '{ExpenseTitle}' for user {Username}", 
                    id, expense.Title, username);

                return Ok(response);
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

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        // UPDATED: Secure search method using SearchUserExpenses
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

                _logger.LogDebug("Executing secure expense search for user {Username}", username);
                
                // SECURITY FIX: Use the new secure method that filters by user
                var results = await _expenseService.SearchUserExpenses(search, username);

                var isAdmin = User.IsInRole("Admin");
                _logger.LogDebug("User {Username} has admin role: {IsAdmin}, results count: {ResultCount}",
                    username, isAdmin, results.Count);

                // Additional security check - ensure no other user's data leaked through
                var unauthorizedExpenses = results.Where(e => e.Username != username).ToList();
                if (unauthorizedExpenses.Any())
                {
                    _logger.LogError("SECURITY ALERT: Search returned {Count} expenses belonging to other users for user {Username}",
                        unauthorizedExpenses.Count, username);
                    return StatusCode(500, "Security error occurred. Please contact support.");
                }

                _logger.LogInformation("Search completed for user {Username}, returning {ResultCount} expenses",
                    username, results.Count);

                return Ok(results.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during expense search by user {Username}", username);
                return BadRequest(ex.Message);
            }
        }

        // ✅ UPDATED: Secure GetAllExpenses method using GetExpensesByUsername with DEBUG logging
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

                // SECURITY: Non-admin users can only access their own expenses
                if (!string.IsNullOrWhiteSpace(username) && username != requester && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized access attempt - user {Requester} tried to access expenses for {TargetUsername}",
                        requester, username);
                    return Unauthorized("You are not authorized to access other users' expenses.");
                }

                var userToQuery = string.IsNullOrWhiteSpace(username) ? requester : username;
                _logger.LogDebug("Querying expenses for user {UserToQuery}", userToQuery);

                var userExpenses = await _expenseService.GetExpensesByUsername(userToQuery, pageNumber, pageSize);

                // ✅ DEBUG: Add temporary logging to verify receipt data
                _logger.LogInformation("=== EXPENSE RECEIPT DEBUG ===");
                _logger.LogInformation("Total expenses returned for user {Username}: {Count}", userToQuery, userExpenses.Count);

                foreach (var expense in userExpenses.Take(5)) // Log first 5 expenses for debugging
                {
                    _logger.LogInformation("Expense {ExpenseId} '{Title}': HasReceipt={HasReceipt}, ReceiptId={ReceiptId}",
                        expense.Id, expense.Title, expense.Receipt != null, expense.Receipt?.Id);
                        
                    if (expense.Receipt != null)
                    {
                        _logger.LogInformation("  Receipt details: Name={Name}, Category={Category}, Created={Created}",
                            expense.Receipt.ReceiptName, expense.Receipt.Category, expense.Receipt.CreatedAt);
                    }
                }

                var unauthorizedExpenses = userExpenses.Where(e => e.Username != userToQuery).ToList();
                if (unauthorizedExpenses.Any())
                {
                    _logger.LogError("SECURITY ALERT: GetExpensesByUsername returned {Count} expenses belonging to other users",
                        unauthorizedExpenses.Count);
                    return StatusCode(500, "Security error occurred. Please contact support.");
                }

                _logger.LogInformation("Retrieved {ReturnedCount} expenses for user {UserToQuery} (Page {PageNumber})",
                    userExpenses.Count, userToQuery, pageNumber);

                // Return only the user's expenses - no risk of data leakage
                return Ok(userExpenses.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving all expenses for requester {Requester} targeting {TargetUsername}",
                    requester, username);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<ActionResult<ICollection<Expense>>> GetAllExpensesAdmin(
            [FromQuery] string? username,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var requester = User.Identity?.Name;
            using var scope = _logger.BeginScope("GetAllExpensesAdmin by {Requester} for {TargetUser} page {PageNumber}",
                requester, username, pageNumber);

            _logger.LogInformation("Admin get all expenses request by {Requester} for user {TargetUsername} (Page: {PageNumber}, Size: {PageSize})",
                requester, username, pageNumber, pageSize);

            try
            {
                if (string.IsNullOrWhiteSpace(requester))
                {
                    return Unauthorized("Username not found in token.");
                }

                if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Invalid pagination parameters.");
                }

                ICollection<Expense> expenses;

                if (!string.IsNullOrWhiteSpace(username))
                {
                    expenses = await _expenseService.GetExpensesByUsername(username, pageNumber, pageSize);
                }
                else
                {
                    var searchModel = new ExpenseSearchModel();
                    var allExpenses = await _expenseService.SearchExpense(searchModel);
                    expenses = allExpenses.OrderByDescending(e => e.CreatedAt)
                                         .Skip((pageNumber - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToList();
                }

                _logger.LogInformation("Admin retrieved {ReturnedCount} expenses for user {TargetUsername}",
                    expenses.Count, username ?? "ALL");

                return Ok(expenses.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in admin get all expenses by {Requester} for {TargetUsername}",
                    requester, username);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/search")]
        public async Task<ActionResult<ICollection<Expense>>> AdminSearchExpenses([FromBody] ExpenseSearchModel search)
        {
            var username = User.Identity?.Name;
            using var scope = _logger.BeginScope("AdminSearchExpenses by {Username}", username);

            _logger.LogInformation("Admin search expenses request by user {Username} with criteria: Title={Title}, Category={Category}",
                username, search?.Title, search?.Category);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Unauthorized("Username not found in token.");
                }

                if (search == null)
                {
                    search = new ExpenseSearchModel();
                }

                var results = await _expenseService.SearchExpense(search);

                _logger.LogInformation("Admin search completed by user {Username}, returning {ResultCount} expenses",
                    username, results.Count);

                return Ok(results.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during admin expense search by user {Username}", username);
                return BadRequest(ex.Message);
            }
        }
    }
}