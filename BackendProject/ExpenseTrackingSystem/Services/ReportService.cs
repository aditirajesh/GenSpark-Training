using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackingSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditService;
        private readonly ILogger<ReportService> _logger; 

        public ReportService(
            IExpenseService expenseService,
            IUserService userService,
            IAuditLogService auditService,
            ILogger<ReportService> logger) 
        {
            _expenseService = expenseService;
            _userService = userService;
            _auditService = auditService;
            _logger = logger; 
        }

        public async Task<ReportSummaryDto> GetQuickSummaryAsync(string requestingUser, string? targetUsername = null, int lastNDays = 30)
        {
            using var scope = _logger.BeginScope("QuickSummary for {RequestingUser} targeting {TargetUser} for {Days} days", 
                requestingUser, targetUsername ?? requestingUser, lastNDays);

            _logger.LogInformation("Starting quick summary report generation for {LastNDays} days", lastNDays);

            var targetUser = await ValidateAndGetTargetUser(requestingUser, targetUsername);
            var startDate = DateTime.Now.AddDays(-lastNDays);
            var endDate = DateTime.Now;

            _logger.LogDebug("Date range calculated: {StartDate} to {EndDate}", startDate, endDate);

            await _auditService.LogAction(new AuditAddRequestDto
            {
                Username = requestingUser,
                Action = "Generate Report",
                EntityName = "Report",
                Details = $"Generating quick summary report for user: {targetUser}",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogDebug("Creating search model for date range filtering");
            var searchModel = new ExpenseSearchModel
            {
                dateRange = new Range<DateTimeOffset>
                {
                    MinVal = new DateTimeOffset(startDate),
                    MaxVal = new DateTimeOffset(endDate)
                }
            };

            _logger.LogDebug("Searching expenses with date filter");
            var allExpenses = await _expenseService.SearchExpense(searchModel);
            var expenses = allExpenses.Where(e => e.Username == targetUser).ToList();

            _logger.LogInformation("Found {ExpenseCount} expenses for user {TargetUser} in the specified date range", 
                expenses.Count, targetUser);

            if (!expenses.Any())
            {
                _logger.LogInformation("No expenses found for user {TargetUser}, returning empty summary", targetUser);
                return new ReportSummaryDto
                {
                    ReportType = "Quick Summary",
                    Username = targetUser,
                    CreatedAt = DateTimeOffset.Now,
                    CreatedBy = requestingUser,
                    Timeline = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    Period = $"Last {lastNDays} days"
                };
            }

            _logger.LogDebug("Calculating summary statistics for {ExpenseCount} expenses", expenses.Count);
            var totalAmount = expenses.Sum(e => e.Amount);
            var categoryGroups = expenses.GroupBy(e => e.Category).ToList();
            var topCategory = categoryGroups.OrderByDescending(g => g.Sum(e => e.Amount)).FirstOrDefault();

            _logger.LogDebug("Summary calculated: Total={TotalAmount}, Average={AverageAmount}, TopCategory={TopCategory}", 
                totalAmount, 
                expenses.Count > 0 ? totalAmount / expenses.Count : 0,
                topCategory?.Key ?? "N/A");

            _logger.LogInformation("Successfully generated quick summary report for user {TargetUser} with {ExpenseCount} expenses totaling {TotalAmount}", 
                targetUser, expenses.Count, totalAmount);

            return new ReportSummaryDto
            {
                ReportType = "Quick Summary",
                Username = targetUser,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = requestingUser,
                Timeline = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                TotalExpense = totalAmount,
                TotalExpenseCount = expenses.Count,
                AverageExpenseAmount = expenses.Count > 0 ? totalAmount / expenses.Count : 0,
                TopCategory = topCategory?.Key ?? "N/A",
                TopCategoryAmount = topCategory?.Sum(e => e.Amount) ?? 0,
                Period = $"Last {lastNDays} days"
            };
        }

        public async Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(string requestingUser, DateTime startDate, DateTime endDate, string? targetUsername = null)
        {
            using var scope = _logger.BeginScope("CategoryBreakdown for {RequestingUser} targeting {TargetUser} from {StartDate} to {EndDate}", 
                requestingUser, targetUsername ?? requestingUser, startDate, endDate);

            _logger.LogInformation("Starting category breakdown report generation");

            var targetUser = await ValidateAndGetTargetUser(requestingUser, targetUsername);

            await _auditService.LogAction(new AuditAddRequestDto
            {
                Username = requestingUser,
                Action = "Generate Report",
                EntityName = "Report",
                Details = $"Generating category breakdown report for user: {targetUser} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogDebug("Creating search model for category breakdown");
            var searchModel = new ExpenseSearchModel
            {
                dateRange = new Range<DateTimeOffset>
                {
                    MinVal = new DateTimeOffset(startDate),
                    MaxVal = new DateTimeOffset(endDate)
                }
            };

            _logger.LogDebug("Searching expenses for category analysis");
            var allExpenses = await _expenseService.SearchExpense(searchModel);
            var expenses = allExpenses.Where(e => e.Username == targetUser).ToList();

            _logger.LogInformation("Found {ExpenseCount} expenses for category breakdown analysis", expenses.Count);

            if (!expenses.Any())
            {
                _logger.LogInformation("No expenses found for category breakdown, returning empty list");
                return new List<CategoryBreakdownDto>();
            }

            _logger.LogDebug("Calculating category breakdown for {ExpenseCount} expenses", expenses.Count);
            var totalAmount = expenses.Sum(e => e.Amount);
            var categoryGroups = expenses.GroupBy(e => e.Category).ToList();
            
            _logger.LogDebug("Found {CategoryCount} unique categories with total amount {TotalAmount}", 
                categoryGroups.Count, totalAmount);

            var categoryBreakdown = expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryBreakdownDto
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    ExpenseCount = g.Count(),
                    AverageAmount = g.Average(e => e.Amount),
                    Percentage = totalAmount > 0 ? Math.Round((g.Sum(e => e.Amount) / totalAmount) * 100, 2) : 0
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            _logger.LogInformation("Successfully generated category breakdown with {CategoryCount} categories for user {TargetUser}", 
                categoryBreakdown.Count, targetUser);

            _logger.LogDebug("Top categories: {TopCategories}", 
                string.Join(", ", categoryBreakdown.Take(3).Select(c => $"{c.Category}({c.TotalAmount:C})")));

            return categoryBreakdown;
        }

        public async Task<List<TimeBasedReportDto>> GetTimeBasedReportAsync(string requestingUser, DateTime startDate, DateTime endDate, string? targetUsername = null, string groupBy = "month")
        {
            using var scope = _logger.BeginScope("TimeBasedReport for {RequestingUser} targeting {TargetUser} grouped by {GroupBy}", 
                requestingUser, targetUsername ?? requestingUser, groupBy);

            _logger.LogInformation("Starting time-based report generation grouped by {GroupBy}", groupBy);

            var targetUser = await ValidateAndGetTargetUser(requestingUser, targetUsername);

            await _auditService.LogAction(new AuditAddRequestDto
            {
                Username = requestingUser,
                Action = "Generate Report",
                EntityName = "Report",
                Details = $"Generating time-based report for user: {targetUser} grouped by {groupBy} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogDebug("Creating search model for time-based analysis");
            var searchModel = new ExpenseSearchModel
            {
                dateRange = new Range<DateTimeOffset>
                {
                    MinVal = new DateTimeOffset(startDate),
                    MaxVal = new DateTimeOffset(endDate)
                }
            };

            _logger.LogDebug("Searching expenses for time-based analysis");
            var allExpenses = await _expenseService.SearchExpense(searchModel);
            var expenses = allExpenses.Where(e => e.Username == targetUser).ToList();

            _logger.LogInformation("Found {ExpenseCount} expenses for time-based analysis", expenses.Count);

            if (!expenses.Any())
            {
                _logger.LogInformation("No expenses found for time-based report, returning empty list");
                return new List<TimeBasedReportDto>();
            }

            _logger.LogDebug("Grouping {ExpenseCount} expenses by {GroupBy}", expenses.Count, groupBy);
            IEnumerable<IGrouping<string, Expense>> timeGroups;

            switch (groupBy.ToLower())
            {
                case "week":
                    _logger.LogTrace("Grouping expenses by week");
                    timeGroups = expenses.GroupBy(e => $"{e.CreatedAt.Year}-W{GetWeekOfYear(e.CreatedAt.DateTime)}");
                    break;
                case "day":
                    _logger.LogTrace("Grouping expenses by day");
                    timeGroups = expenses.GroupBy(e => e.CreatedAt.ToString("yyyy-MM-dd"));
                    break;
                case "year":
                    _logger.LogTrace("Grouping expenses by year");
                    timeGroups = expenses.GroupBy(e => e.CreatedAt.Year.ToString());
                    break;
                default: // month
                    _logger.LogTrace("Grouping expenses by month (default)");
                    timeGroups = expenses.GroupBy(e => e.CreatedAt.ToString("yyyy-MM"));
                    break;
            }

            var timeGroupsList = timeGroups.ToList();
            _logger.LogDebug("Created {TimeGroupCount} time groups", timeGroupsList.Count);

            var timeBasedReport = timeGroups
                .Select(g => new TimeBasedReportDto
                {
                    TimePeriod = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    ExpenseCount = g.Count(),
                    AverageAmount = g.Average(e => e.Amount),
                    TopCategories = g.GroupBy(e => e.Category)
                        .Select(cg => new CategoryBreakdownDto
                        {
                            Category = cg.Key,
                            TotalAmount = cg.Sum(e => e.Amount),
                            ExpenseCount = cg.Count(),
                            AverageAmount = cg.Average(e => e.Amount),
                            Percentage = Math.Round((cg.Sum(e => e.Amount) / g.Sum(e => e.Amount)) * 100, 2)
                        })
                        .OrderByDescending(c => c.TotalAmount)
                        .Take(3)
                        .ToList()
                })
                .OrderBy(t => t.TimePeriod)
                .ToList();

            _logger.LogInformation("Successfully generated time-based report with {PeriodCount} time periods for user {TargetUser}", 
                timeBasedReport.Count, targetUser);

            _logger.LogDebug("Time periods: {TimePeriods}", 
                string.Join(", ", timeBasedReport.Select(t => $"{t.TimePeriod}({t.ExpenseCount} expenses)")));

            return timeBasedReport;
        }

        public async Task<List<TopExpenseDto>> GetTopExpensesAsync(string requestingUser, DateTime startDate, DateTime endDate, string? targetUsername = null, int limit = 10)
        {
            using var scope = _logger.BeginScope("TopExpenses for {RequestingUser} targeting {TargetUser} limit {Limit}", 
                requestingUser, targetUsername ?? requestingUser, limit);

            _logger.LogInformation("Starting top expenses report generation with limit {Limit}", limit);

            var targetUser = await ValidateAndGetTargetUser(requestingUser, targetUsername);

            await _auditService.LogAction(new AuditAddRequestDto
            {
                Username = requestingUser,
                Action = "Generate Report",
                EntityName = "Report",
                Details = $"Generating top {limit} expenses report for user: {targetUser} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogDebug("Creating search model for top expenses analysis");
            var searchModel = new ExpenseSearchModel
            {
                dateRange = new Range<DateTimeOffset>
                {
                    MinVal = new DateTimeOffset(startDate),
                    MaxVal = new DateTimeOffset(endDate)
                }
            };

            _logger.LogDebug("Searching expenses for top expenses analysis");
            var allExpenses = await _expenseService.SearchExpense(searchModel);
            var expenses = allExpenses.Where(e => e.Username == targetUser).ToList();

            _logger.LogInformation("Found {ExpenseCount} expenses, selecting top {Limit} by amount", expenses.Count, limit);

            var topExpenses = expenses
                .OrderByDescending(e => e.Amount)
                .Take(limit)
                .Select(e => new TopExpenseDto
                {
                    ExpenseId = e.Id,
                    Title = e.Title,
                    Category = e.Category,
                    Amount = e.Amount,
                    CreatedAt = e.CreatedAt,
                    Notes = e.Notes,
                    HasReceipt = e.Receipt != null
                })
                .ToList();

            _logger.LogInformation("Successfully generated top expenses report with {ReturnedCount} expenses for user {TargetUser}", 
                topExpenses.Count, targetUser);

            if (topExpenses.Any())
            {
                _logger.LogDebug("Top expense amounts: {TopAmounts}", 
                    string.Join(", ", topExpenses.Take(5).Select(e => $"{e.Amount:C}")));
            }

            return topExpenses;
        }

        public async Task<DetailedReportDto> GetDetailedReportAsync(string requestingUser, ReportRequestDto request)
        {
            using var scope = _logger.BeginScope("DetailedReport for {RequestingUser} targeting {TargetUser}", 
                requestingUser, request.Username ?? requestingUser);

            _logger.LogInformation("Starting detailed report generation from {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);

            var targetUser = await ValidateAndGetTargetUser(requestingUser, request.Username);

            _logger.LogDebug("Generating summary component of detailed report");
            var summary = await GetQuickSummaryAsync(requestingUser, targetUser, (int)(request.EndDate - request.StartDate).TotalDays);
            
            _logger.LogDebug("Generating category breakdown component of detailed report");
            var categoryBreakdown = await GetCategoryBreakdownAsync(requestingUser, request.StartDate, request.EndDate, targetUser);
            
            _logger.LogDebug("Generating time-based component of detailed report");
            var timeBasedData = await GetTimeBasedReportAsync(requestingUser, request.StartDate, request.EndDate, targetUser);
            
            _logger.LogDebug("Generating top expenses component of detailed report with limit {Limit}", request.TopExpensesLimit);
            var topExpenses = await GetTopExpensesAsync(requestingUser, request.StartDate, request.EndDate, targetUser, request.TopExpensesLimit);

            _logger.LogInformation("Successfully compiled detailed report for user {TargetUser} with {CategoryCount} categories, {TimePeriodsCount} time periods, and {TopExpensesCount} top expenses", 
                targetUser, categoryBreakdown.Count, timeBasedData.Count, topExpenses.Count);

            return new DetailedReportDto
            {
                Summary = summary,
                CategoryBreakdown = categoryBreakdown,
                TimeBasedData = timeBasedData,
                TopExpenses = topExpenses
            };
        }

        private async Task<string> ValidateAndGetTargetUser(string requestingUser, string? targetUsername)
        {
            _logger.LogDebug("Validating user access: {RequestingUser} requesting data for {TargetUser}", 
                requestingUser, targetUsername ?? requestingUser);

            var requestingUserData = await _userService.GetUserByUsername(requestingUser);
            if (requestingUserData == null)
            {
                _logger.LogWarning("Invalid requesting user: {RequestingUser}", requestingUser);
                throw new UnauthorizedAccessException("Invalid requesting user.");
            }

            if (string.IsNullOrEmpty(targetUsername))
            {
                _logger.LogDebug("No target username specified, using requesting user {RequestingUser}", requestingUser);
                return requestingUser;
            }

            if (requestingUserData.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Admin user {RequestingUser} accessing data for {TargetUser}", requestingUser, targetUsername);
                var targetUserData = await _userService.GetUserByUsername(targetUsername);
                if (targetUserData == null)
                {
                    _logger.LogWarning("Target user {TargetUser} not found for admin request", targetUsername);
                    throw new ArgumentException($"Target user '{targetUsername}' not found.");
                }
                _logger.LogDebug("Admin access granted for user {TargetUser}", targetUsername);
                return targetUsername;
            }

            if (!requestingUser.Equals(targetUsername, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Unauthorized access attempt: {RequestingUser} tried to access {TargetUser}'s data", 
                    requestingUser, targetUsername);
                throw new UnauthorizedAccessException("You can only access your own reports.");
            }

            _logger.LogDebug("User access validated: {RequestingUser} accessing own data", requestingUser);
            return requestingUser;
        }

        private static int GetWeekOfYear(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }
    }
}