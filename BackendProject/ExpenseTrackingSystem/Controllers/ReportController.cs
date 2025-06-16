using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseTrackingSystem.Services;
using ExpenseTrackingSystem.Exceptions;
using System.Security.Claims;
using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Interfaces;
using Microsoft.Extensions.Logging; // Add this using statement

namespace ExpenseTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger; // Add this field

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger) // Add logger parameter
        {
            _reportService = reportService;
            _logger = logger; // Assign logger
        }

        private string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value 
                ?? User.FindFirst("username")?.Value 
                ?? throw new UnauthorizedAccessException("Username not found in token.");
        }

        [HttpPost("quick-summary")]
        public async Task<ActionResult<ReportSummaryDto>> GetQuickSummary([FromBody] QuickSummaryRequestDto request)
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("QuickSummaryReport by {CurrentUser} for {TargetUser} days {Days}", 
                currentUser, request?.Username ?? currentUser, request?.LastNDays);

            _logger.LogInformation("Quick summary report requested by {CurrentUser} for user {TargetUser} for last {Days} days", 
                currentUser, request?.Username, request?.LastNDays);

            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Quick summary request failed - null request body from user {CurrentUser}", currentUser);
                    return BadRequest(new { message = "Invalid request body." });
                }

                if (request.LastNDays <= 0 || request.LastNDays > 365)
                {
                    _logger.LogWarning("Quick summary request failed - invalid days parameter {Days} from user {CurrentUser}", 
                        request.LastNDays, currentUser);
                    return BadRequest(new { message = "Days must be between 1 and 365." });
                }

                _logger.LogDebug("Processing quick summary report request");
                var report = await _reportService.GetQuickSummaryAsync(currentUser, request.Username, request.LastNDays);
                
                _logger.LogInformation("Quick summary report generated successfully for user {TargetUser} by {CurrentUser} - {ExpenseCount} expenses, total: {TotalAmount}", 
                    request.Username ?? currentUser, currentUser, report.TotalExpenseCount, report.TotalExpense);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Quick summary report unauthorized - user {CurrentUser} tried to access {TargetUser}: {ErrorMessage}", 
                    currentUser, request?.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Quick summary report bad request from user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Quick summary report not found for user {CurrentUser} targeting {TargetUser}: {ErrorMessage}", 
                    currentUser, request?.Username, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating quick summary report for user {CurrentUser} targeting {TargetUser}", 
                    currentUser, request?.Username);
                return StatusCode(500, new { message = "An error occurred while generating the report.", details = ex.Message });
            }
        }

        [HttpGet("category-breakdown")]
        public async Task<ActionResult<List<CategoryBreakdownDto>>> GetCategoryBreakdown(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? username = null)
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("CategoryBreakdownReport by {CurrentUser} for {TargetUser} from {StartDate} to {EndDate}", 
                currentUser, username ?? currentUser, startDate, endDate);

            _logger.LogInformation("Category breakdown report requested by {CurrentUser} for user {TargetUser} from {StartDate} to {EndDate}", 
                currentUser, username, startDate, endDate);

            try
            {
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Category breakdown request failed - invalid date range from user {CurrentUser}: {StartDate} >= {EndDate}", 
                        currentUser, startDate, endDate);
                    return BadRequest(new { message = "Start date must be before end date." });
                }

                var daysDifference = (endDate - startDate).TotalDays;
                if (daysDifference > 365)
                {
                    _logger.LogWarning("Category breakdown request failed - date range too large from user {CurrentUser}: {Days} days", 
                        currentUser, daysDifference);
                    return BadRequest(new { message = "Date range cannot exceed 365 days." });
                }

                _logger.LogDebug("Processing category breakdown report request");
                var report = await _reportService.GetCategoryBreakdownAsync(currentUser, startDate, endDate, username);
                
                _logger.LogInformation("Category breakdown report generated successfully for user {TargetUser} by {CurrentUser} - {CategoryCount} categories found", 
                    username ?? currentUser, currentUser, report.Count);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Category breakdown report unauthorized - user {CurrentUser} tried to access {TargetUser}: {ErrorMessage}", 
                    currentUser, username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Category breakdown report bad request from user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Category breakdown report not found for user {CurrentUser} targeting {TargetUser}: {ErrorMessage}", 
                    currentUser, username, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating category breakdown report for user {CurrentUser} targeting {TargetUser}", 
                    currentUser, username);
                return StatusCode(500, new { message = "An error occurred while generating the category breakdown.", details = ex.Message });
            }
        }

        [HttpGet("time-based")]
        public async Task<ActionResult<List<TimeBasedReportDto>>> GetTimeBasedReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string groupBy = "month",
            [FromQuery] string? username = null)
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("TimeBasedReport by {CurrentUser} for {TargetUser} grouped by {GroupBy}", 
                currentUser, username ?? currentUser, groupBy);

            _logger.LogInformation("Time-based report requested by {CurrentUser} for user {TargetUser} from {StartDate} to {EndDate} grouped by {GroupBy}", 
                currentUser, username, startDate, endDate, groupBy);

            try
            {
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Time-based report failed - invalid date range from user {CurrentUser}: {StartDate} >= {EndDate}", 
                        currentUser, startDate, endDate);
                    return BadRequest(new { message = "Start date must be before end date." });
                }

                var validGroupBy = new[] { "day", "week", "month", "year" };
                if (!validGroupBy.Contains(groupBy.ToLower()))
                {
                    _logger.LogWarning("Time-based report failed - invalid groupBy parameter from user {CurrentUser}: {GroupBy}", 
                        currentUser, groupBy);
                    return BadRequest(new { message = "GroupBy must be one of: day, week, month, year" });
                }

                _logger.LogDebug("Processing time-based report request with groupBy: {GroupBy}", groupBy);
                var report = await _reportService.GetTimeBasedReportAsync(currentUser, startDate, endDate, username, groupBy);
                
                _logger.LogInformation("Time-based report generated successfully for user {TargetUser} by {CurrentUser} - {PeriodCount} time periods found", 
                    username ?? currentUser, currentUser, report.Count);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Time-based report unauthorized - user {CurrentUser} tried to access {TargetUser}: {ErrorMessage}", 
                    currentUser, username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Time-based report bad request from user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Time-based report not found for user {CurrentUser} targeting {TargetUser}: {ErrorMessage}", 
                    currentUser, username, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating time-based report for user {CurrentUser} targeting {TargetUser}", 
                    currentUser, username);
                return StatusCode(500, new { message = "An error occurred while generating the time-based report.", details = ex.Message });
            }
        }

        [HttpGet("top-expenses")]
        public async Task<ActionResult<List<TopExpenseDto>>> GetTopExpenses(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int limit = 10,
            [FromQuery] string? username = null)
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("TopExpensesReport by {CurrentUser} for {TargetUser} limit {Limit}", 
                currentUser, username ?? currentUser, limit);

            _logger.LogInformation("Top expenses report requested by {CurrentUser} for user {TargetUser} from {StartDate} to {EndDate} with limit {Limit}", 
                currentUser, username, startDate, endDate, limit);

            try
            {
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Top expenses report failed - invalid date range from user {CurrentUser}: {StartDate} >= {EndDate}", 
                        currentUser, startDate, endDate);
                    return BadRequest(new { message = "Start date must be before end date." });
                }

                if (limit <= 0 || limit > 100)
                {
                    _logger.LogWarning("Top expenses report failed - invalid limit from user {CurrentUser}: {Limit}", 
                        currentUser, limit);
                    return BadRequest(new { message = "Limit must be between 1 and 100." });
                }

                _logger.LogDebug("Processing top expenses report request with limit: {Limit}", limit);
                var report = await _reportService.GetTopExpensesAsync(currentUser, startDate, endDate, username, limit);
                
                _logger.LogInformation("Top expenses report generated successfully for user {TargetUser} by {CurrentUser} - {ExpenseCount} expenses returned", 
                    username ?? currentUser, currentUser, report.Count);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Top expenses report unauthorized - user {CurrentUser} tried to access {TargetUser}: {ErrorMessage}", 
                    currentUser, username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Top expenses report bad request from user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Top expenses report not found for user {CurrentUser} targeting {TargetUser}: {ErrorMessage}", 
                    currentUser, username, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating top expenses report for user {CurrentUser} targeting {TargetUser}", 
                    currentUser, username);
                return StatusCode(500, new { message = "An error occurred while generating the top expenses report.", details = ex.Message });
            }
        }

        [HttpPost("detailed")]
        public async Task<ActionResult<DetailedReportDto>> GetDetailedReport([FromBody] ReportRequestDto request)
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("DetailedReport by {CurrentUser} for {TargetUser}", 
                currentUser, request?.Username ?? currentUser);

            _logger.LogInformation("Detailed report requested by {CurrentUser} for user {TargetUser} from {StartDate} to {EndDate}", 
                currentUser, request?.Username, request?.StartDate, request?.EndDate);

            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Detailed report request failed - null request body from user {CurrentUser}", currentUser);
                    return BadRequest(new { message = "Invalid request body." });
                }

                if (request.StartDate >= request.EndDate)
                {
                    _logger.LogWarning("Detailed report failed - invalid date range from user {CurrentUser}: {StartDate} >= {EndDate}", 
                        currentUser, request.StartDate, request.EndDate);
                    return BadRequest(new { message = "Start date must be before end date." });
                }

                if (request.TopExpensesLimit <= 0 || request.TopExpensesLimit > 100)
                {
                    _logger.LogDebug("Adjusting invalid TopExpensesLimit from {InvalidLimit} to 10 for user {CurrentUser}", 
                        request.TopExpensesLimit, currentUser);
                    request.TopExpensesLimit = 10; // Set default
                }

                _logger.LogDebug("Processing detailed report request with TopExpensesLimit: {Limit}", request.TopExpensesLimit);
                var report = await _reportService.GetDetailedReportAsync(currentUser, request);
                
                _logger.LogInformation("Detailed report generated successfully for user {TargetUser} by {CurrentUser} - Summary: {ExpenseCount} expenses, {CategoryCount} categories, {TimePeriodsCount} periods, {TopExpensesCount} top expenses", 
                    request.Username ?? currentUser, currentUser, 
                    report.Summary.TotalExpenseCount, 
                    report.CategoryBreakdown.Count, 
                    report.TimeBasedData.Count, 
                    report.TopExpenses.Count);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Detailed report unauthorized - user {CurrentUser} tried to access {TargetUser}: {ErrorMessage}", 
                    currentUser, request?.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Detailed report bad request from user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Detailed report not found for user {CurrentUser} targeting {TargetUser}: {ErrorMessage}", 
                    currentUser, request?.Username, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating detailed report for user {CurrentUser} targeting {TargetUser}", 
                    currentUser, request?.Username);
                return StatusCode(500, new { message = "An error occurred while generating the detailed report.", details = ex.Message });
            }
        }

        [HttpGet("current-month-summary")]
        public async Task<ActionResult<ReportSummaryDto>> GetCurrentMonthSummary()
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("CurrentMonthSummary by {CurrentUser}", currentUser);

            _logger.LogInformation("Current month summary report requested by {CurrentUser}", currentUser);

            try
            {
                var currentDate = DateTime.Now;
                var daysInCurrentMonth = (currentDate - new DateTime(currentDate.Year, currentDate.Month, 1)).Days + 1;
                
                _logger.LogDebug("Calculating current month summary for {Days} days in {Month}/{Year}", 
                    daysInCurrentMonth, currentDate.Month, currentDate.Year);
                
                var report = await _reportService.GetQuickSummaryAsync(currentUser, null, daysInCurrentMonth);
                
                _logger.LogInformation("Current month summary generated successfully for user {CurrentUser} - {ExpenseCount} expenses, total: {TotalAmount}", 
                    currentUser, report.TotalExpenseCount, report.TotalExpense);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Current month summary unauthorized for user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Current month summary not found for user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating current month summary for user {CurrentUser}", currentUser);
                return StatusCode(500, new { message = "An error occurred while generating the current month summary.", details = ex.Message });
            }
        }

        [HttpGet("my-categories")]
        public async Task<ActionResult<List<CategoryBreakdownDto>>> GetMyCategories()
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("MyCategories by {CurrentUser}", currentUser);

            _logger.LogInformation("My categories report requested by {CurrentUser} for last 30 days", currentUser);

            try
            {
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);
                
                _logger.LogDebug("Generating category breakdown for user {CurrentUser} from {StartDate} to {EndDate}", 
                    currentUser, startDate, endDate);
                
                var report = await _reportService.GetCategoryBreakdownAsync(currentUser, startDate, endDate);
                
                _logger.LogInformation("My categories report generated successfully for user {CurrentUser} - {CategoryCount} categories found", 
                    currentUser, report.Count);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("My categories report unauthorized for user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("My categories report not found for user {CurrentUser}: {ErrorMessage}", 
                    currentUser, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating my categories report for user {CurrentUser}", currentUser);
                return StatusCode(500, new { message = "An error occurred while generating your category breakdown.", details = ex.Message });
            }
        }

        [HttpGet("admin/user-summary/{targetUsername}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReportSummaryDto>> GetUserSummaryForAdmin(
            string targetUsername,
            [FromQuery] int lastNDays = 30)
        {
            var currentUser = GetCurrentUsername();
            using var scope = _logger.BeginScope("AdminUserSummary by {AdminUser} for {TargetUser} days {Days}", 
                currentUser, targetUsername, lastNDays);

            _logger.LogInformation("Admin user summary requested by admin {AdminUser} for user {TargetUser} for last {Days} days", 
                currentUser, targetUsername, lastNDays);

            try
            {
                if (string.IsNullOrWhiteSpace(targetUsername))
                {
                    _logger.LogWarning("Admin user summary failed - empty target username from admin {AdminUser}", currentUser);
                    return BadRequest(new { message = "Target username is required." });
                }

                if (lastNDays <= 0 || lastNDays > 365)
                {
                    _logger.LogWarning("Admin user summary failed - invalid days parameter {Days} from admin {AdminUser}", 
                        lastNDays, currentUser);
                    return BadRequest(new { message = "Days must be between 1 and 365." });
                }

                _logger.LogDebug("Processing admin user summary request for {TargetUser}", targetUsername);
                var report = await _reportService.GetQuickSummaryAsync(currentUser, targetUsername, lastNDays);
                
                _logger.LogInformation("Admin user summary generated successfully by admin {AdminUser} for user {TargetUser} - {ExpenseCount} expenses, total: {TotalAmount}", 
                    currentUser, targetUsername, report.TotalExpenseCount, report.TotalExpense);
                
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Admin user summary unauthorized - admin {AdminUser} tried to access {TargetUser}: {ErrorMessage}", 
                    currentUser, targetUsername, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Admin user summary bad request from admin {AdminUser} for {TargetUser}: {ErrorMessage}", 
                    currentUser, targetUsername, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Admin user summary not found - admin {AdminUser} requested {TargetUser}: {ErrorMessage}", 
                    currentUser, targetUsername, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating admin user summary by admin {AdminUser} for user {TargetUser}", 
                    currentUser, targetUsername);
                return StatusCode(500, new { message = "An error occurred while generating the user summary.", details = ex.Message });
            }
        }
    }
}