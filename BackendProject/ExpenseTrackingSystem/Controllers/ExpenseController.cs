using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Misc;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/expenses")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;

        public ExpenseController(IExpenseService expenseService,
                                IUserService userService)
        {
            _expenseService = expenseService;
            _userService = userService;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<Expense>> AddExpense([FromForm] ExpenseAddRequestDto dto)
        {
            try
            {
                var currentUsername = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(currentUsername))
                    return Unauthorized("Username not found in token.");

                var isAdmin = User.IsInRole("Admin");
                
                // Determine target username
                string targetUsername;
                
                if (!string.IsNullOrWhiteSpace(dto.TargetUsername))
                {
                    // Admin trying to add expense for another user
                    if (!isAdmin)
                        return Unauthorized("Only administrators can add expenses for other users.");
                    
                    targetUsername = dto.TargetUsername;
                }
                else
                {
                    // Default: add expense for current user
                    targetUsername = currentUsername;
                }

                // Pass both target username and current username for audit tracking
                var result = await _expenseService.AddExpense(dto, targetUsername, currentUsername);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Expense>> UpdateExpense([FromForm] ExpenseUpdateRequestDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var expense = await _expenseService.GetExpense(dto.Id);
                if (expense.Username != username && !User.IsInRole("Admin"))
                    return Unauthorized("You can only update your own expenses.");

                var result = await _expenseService.UpdateExpense(dto, username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<Expense>> DeleteExpense(Guid id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var expense = await _expenseService.GetExpense(id);
                if (expense.Username != username && !User.IsInRole("Admin"))
                    return Unauthorized("You can only delete your own expenses.");

                // Pass the current username to the service
                var result = await _expenseService.DeleteExpense(id,username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get/{id}")]
        public async Task<ActionResult<Expense>> GetExpense(Guid id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var expense = await _expenseService.GetExpense(id);
                if (expense.Username != username && !User.IsInRole("Admin"))
                    return Unauthorized("You can only view your own expenses.");

                return Ok(expense);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("search")]
        public async Task<ActionResult<ICollection<Expense>>> SearchExpenses([FromBody] ExpenseSearchModel search)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var results = await _expenseService.SearchExpense(search);
                if (!User.IsInRole("Admin"))
                    results = results.Where(e => e.Username == username).ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
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
            try
            {
                var requester = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(requester))
                    return Unauthorized("Username not found in token.");

                if (!string.IsNullOrWhiteSpace(username) && username != requester && !User.IsInRole("Admin"))
                    return Unauthorized("You are not authorized to access other users' expenses.");

                var userToQuery = string.IsNullOrWhiteSpace(username) ? requester : username;

                var results = await _expenseService.SearchExpense(new ExpenseSearchModel());
                var filtered = results
                                .Where(e => e.Username == userToQuery)
                                .OrderByDescending(e => e.CreatedAt)
                                .Paginate(pageNumber, pageSize)
                                .ToList();

                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
