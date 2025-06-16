using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.Extensions.Logging; // Add this using statement

namespace ExpenseTrackingSystem.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IRepository<Guid, Expense> _expenseRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly ExpenseMapper _expenseMapper;
        private readonly IReceiptService _receiptService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<ExpenseService> _logger; // Add this field

        public ExpenseService(
            IRepository<Guid, Expense> expenseRepository,
            IRepository<string, User> userRepository,
            IReceiptService receiptService,
            IAuditLogService auditLogService,
            ILogger<ExpenseService> logger) // Add logger parameter
        {
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _expenseMapper = new();
            _receiptService = receiptService;
            _auditLogService = auditLogService;
            _logger = logger; // Assign logger
        }

        public async Task<Expense> AddExpense(ExpenseAddRequestDto dto, string targetUsername, string? createdByUsername = null)
        {
            _logger.LogInformation("Starting expense creation for user {TargetUsername} with amount {Amount} and category {Category}", 
                targetUsername, dto?.Amount, dto?.Category);

            if (dto == null) 
            {
                _logger.LogWarning("Expense DTO is null for user {TargetUsername}", targetUsername);
                throw new ArgumentNullException(nameof(dto), "Expense DTO cannot be null");
            }

            _logger.LogDebug("Looking up user {TargetUsername} in repository", targetUsername);
            var user = await _userRepository.GetByID(targetUsername);
            if (user == null)
            {
                _logger.LogWarning("Target user {TargetUsername} not found during expense creation", targetUsername);
                throw new EntityNotFoundException("Target user not found");
            }

            _logger.LogDebug("Mapping expense DTO to expense entity for user {TargetUsername}", targetUsername);
            var expense = _expenseMapper.MapAddRequestExpense(dto);

            expense.Username = targetUsername;
            expense.User = user;
            expense.CreatedAt = DateTimeOffset.UtcNow;
            expense.CreatedBy = createdByUsername ?? targetUsername;
            expense.UpdatedAt = DateTimeOffset.UtcNow;
            expense.UpdatedBy = createdByUsername ?? targetUsername;

            _logger.LogDebug("Adding expense to repository for user {TargetUsername}", targetUsername);
            var addedExpense = await _expenseRepository.Add(expense);
            if (addedExpense == null)
            {
                _logger.LogError("Failed to add expense to repository for user {TargetUsername}", targetUsername);
                throw new EntityCreationException("Failed to add expense");
            }

            _logger.LogDebug("Updating user {TargetUsername} with new expense {ExpenseId}", targetUsername, addedExpense.Id);
            user.Expenses ??= new List<Expense>();
            user.Expenses.Add(addedExpense);
            await _userRepository.Update(targetUsername, user);

            if (dto.ReceiptBill != null)
            {
                _logger.LogInformation("Processing receipt upload for expense {ExpenseId}", addedExpense.Id);
                var receipt_dto = new ReceiptAddRequestDto
                {
                    ReceiptBill = dto.ReceiptBill,
                    ReceiptName = $"{targetUsername}_{addedExpense.Id}_{dto.Title}_receipt",
                    Username = targetUsername,
                    Category = dto.Category,
                    ExpenseId = addedExpense.Id
                };
                await _receiptService.CreateReceipt(receipt_dto);
                _logger.LogInformation("Receipt successfully created for expense {ExpenseId}", addedExpense.Id);
            }
            else
            {
                _logger.LogDebug("No receipt provided for expense {ExpenseId}", addedExpense.Id);
            }

            _logger.LogDebug("Logging audit action for expense creation {ExpenseId}", addedExpense.Id);
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = createdByUsername ?? targetUsername,
                Action = "Create",
                EntityName = "Expense",
                Details = $"Created Expense '{addedExpense.Title}' (ID: {addedExpense.Id}) for user '{targetUsername}' with amount {addedExpense.Amount}",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Successfully created expense {ExpenseId} for user {TargetUsername} with amount {Amount}", 
                addedExpense.Id, targetUsername, addedExpense.Amount);
            return addedExpense;
        }

        public async Task<Expense> UpdateExpense(ExpenseUpdateRequestDto dto, string username)
        {
            _logger.LogInformation("Starting expense update for expense {ExpenseId} by user {Username}", dto.Id, username);

            var expense = await _expenseRepository.GetByID(dto.Id);
            if (expense == null)
            {
                _logger.LogWarning("Expense {ExpenseId} not found during update attempt by user {Username}", dto.Id, username);
                throw new EntityNotFoundException("Expense not found");
            }

            _logger.LogDebug("Found expense {ExpenseId}, current amount: {CurrentAmount}, updating to: {NewAmount}", 
                dto.Id, expense.Amount, dto.Amount ?? expense.Amount);

            expense.Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : expense.Category;
            expense.Notes = !string.IsNullOrWhiteSpace(dto.Notes) ? dto.Notes : expense.Notes;
            expense.Amount = dto.Amount ?? expense.Amount;

            if (!string.IsNullOrWhiteSpace(dto.Title))
                expense.Title = dto.Title;

            expense.UpdatedAt = DateTimeOffset.UtcNow;
            expense.UpdatedBy = username;

            if (dto.Receipt != null)
            {
                _logger.LogInformation("Processing receipt update for expense {ExpenseId}", dto.Id);
                var receipt_update_dto = new ReceiptUpdateRequestDto
                {
                    Receipt = dto.Receipt,
                    Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : null,
                    ReceiptName = !string.IsNullOrWhiteSpace(dto.Title) ? $"{username}_{expense.Id}_{dto.Title}_receipt" : null
                };

                // Check if expense has a receipt (navigation property)
                if (expense.Receipt != null)
                {
                    _logger.LogDebug("Updating existing receipt for expense {ExpenseId}", dto.Id);
                    var receipt_update = await _receiptService.UpdateReceipt(receipt_update_dto, expense.Receipt.Id);
                    if (receipt_update == null)
                    {
                        _logger.LogError("Failed to update receipt for expense {ExpenseId}", dto.Id);
                        throw new EntityUpdateException("Could not update Receipt for this Expense");
                    }
                    _logger.LogInformation("Successfully updated receipt for expense {ExpenseId}", dto.Id);
                }
                else
                {
                    _logger.LogDebug("Creating new receipt for expense {ExpenseId}", dto.Id);
                    var receipt_add_dto = new ReceiptAddRequestDto
                    {
                        ReceiptBill = dto.Receipt,
                        Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : null,
                        ReceiptName = !string.IsNullOrWhiteSpace(dto.Title) ? $"{username}_{expense.Id}_{dto.Title}_receipt" : $"{username}_{expense.Id}_{expense.Title}",
                        Username = username,
                        ExpenseId = expense.Id
                    };
                    var receipt_add = await _receiptService.CreateReceipt(receipt_add_dto);
                    if (receipt_add == null)
                    {
                        _logger.LogError("Failed to create new receipt for expense {ExpenseId}", dto.Id);
                        throw new EntityCreationException("Could not create Receipt for this Expense");
                    }
                    _logger.LogInformation("Successfully created new receipt for expense {ExpenseId}", dto.Id);
                }
            }
            else
            {
                _logger.LogDebug("No receipt update provided for expense {ExpenseId}", dto.Id);
            }

            _logger.LogDebug("Updating expense {ExpenseId} in repository", dto.Id);
            var updatedExpense = await _expenseRepository.Update(expense.Id, expense);
            if (updatedExpense == null)
            {
                _logger.LogError("Failed to update expense {ExpenseId} in repository", dto.Id);
                throw new EntityUpdateException("Could not update expense");
            }

            _logger.LogDebug("Logging audit action for expense update {ExpenseId}", dto.Id);
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = username,
                Action = "Update",
                EntityName = "Expense",
                Details = "Expense Updated",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Successfully updated expense {ExpenseId} by user {Username}", dto.Id, username);
            return updatedExpense;
        }

        public async Task<Expense> DeleteExpense(Guid id, string currentUsername)
        {
            _logger.LogInformation("Starting expense deletion for expense {ExpenseId} by user {CurrentUsername}", id, currentUsername);

            var expense = await _expenseRepository.GetByID(id);
            if (expense == null)
            {
                _logger.LogWarning("Expense {ExpenseId} not found during deletion attempt by user {CurrentUsername}", id, currentUsername);
                throw new EntityNotFoundException("Expense not found");
            }

            var originalTitle = expense.Title;
            var originalUsername = expense.Username;

            _logger.LogInformation("Found expense {ExpenseId} titled '{ExpenseTitle}' owned by {OriginalUsername}, deleting...", 
                id, originalTitle, originalUsername);

            // Log audit
            _logger.LogDebug("Logging audit action for expense deletion {ExpenseId}", id);
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = currentUsername,
                Action = "Delete",
                EntityName = "Expense",
                Details = $"Hard deleted Expense '{originalTitle}' (ID: {id}) owned by '{originalUsername}'",
                Timestamp = DateTimeOffset.UtcNow
            });

            // Delete expense - receipt will cascade delete automatically
            _logger.LogDebug("Performing hard delete of expense {ExpenseId} from repository", id);
            var deleted_expense = await _expenseRepository.Delete(id);
            Console.WriteLine($"Deleted Expense: {deleted_expense.Id} - {deleted_expense.Title}");

            _logger.LogInformation("Successfully deleted expense {ExpenseId} titled '{ExpenseTitle}' by user {CurrentUsername}", 
                id, originalTitle, currentUsername);
            return deleted_expense;
        }

        public async Task<Expense> GetExpense(Guid id)
        {
            _logger.LogDebug("Retrieving expense {ExpenseId}", id);

            var expense = await _expenseRepository.GetByID(id);
            if (expense == null)
            {
                _logger.LogWarning("Expense {ExpenseId} not found during retrieval", id);
                throw new EntityNotFoundException("Expense not found");
            }

            _logger.LogDebug("Successfully retrieved expense {ExpenseId} titled '{ExpenseTitle}'", id, expense.Title);
            return expense;
        }

        public async Task<ICollection<Expense>> SearchExpense(ExpenseSearchModel searchModel)
        {
            _logger.LogInformation("Starting expense search with criteria: Title={Title}, Category={Category}, AmountRange={AmountRange}, DateRange={DateRange}", 
                searchModel.Title, searchModel.Category, 
                searchModel.amountRange != null ? $"{searchModel.amountRange.MinVal}-{searchModel.amountRange.MaxVal}" : "None",
                searchModel.dateRange != null ? $"{searchModel.dateRange.MinVal}-{searchModel.dateRange.MaxVal}" : "None");

            _logger.LogDebug("Retrieving all expenses from repository");
            var expenses = (await _expenseRepository.GetAll()).ToList();
            _logger.LogDebug("Retrieved {TotalExpenses} expenses from repository", expenses.Count);

            var originalCount = expenses.Count;

            expenses = FilterByTitle(expenses, searchModel.Title);
            _logger.LogDebug("After title filter: {Count} expenses (filtered {Removed})", expenses.Count, originalCount - expenses.Count);

            expenses = FilterByCategory(expenses, searchModel.Category);
            _logger.LogDebug("After category filter: {Count} expenses", expenses.Count);

            expenses = FilterByAmount(expenses, searchModel.amountRange);
            _logger.LogDebug("After amount filter: {Count} expenses", expenses.Count);

            expenses = FilterByDate(expenses, searchModel.dateRange);
            _logger.LogDebug("After date filter: {Count} expenses", expenses.Count);

            var finalExpenses = expenses.OrderByDescending(e => e.CreatedAt).ToList();
            _logger.LogInformation("Search completed, returning {ResultCount} expenses out of {TotalCount} total", 
                finalExpenses.Count, originalCount);

            return finalExpenses;
        }

        private List<Expense> FilterByTitle(List<Expense> expenses, string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) 
            {
                _logger.LogTrace("No title filter applied");
                return expenses;
            }
            
            _logger.LogTrace("Filtering expenses by title containing '{Title}'", title);
            return expenses.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<Expense> FilterByCategory(List<Expense> expenses, string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) 
            {
                _logger.LogTrace("No category filter applied");
                return expenses;
            }
            
            _logger.LogTrace("Filtering expenses by category containing '{Category}'", category);
            return expenses.Where(e => e.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<Expense> FilterByAmount(List<Expense> expenses, Range<decimal>? amountRange)
        {
            if (amountRange == null) 
            {
                _logger.LogTrace("No amount range filter applied");
                return expenses;
            }

            _logger.LogTrace("Filtering expenses by amount range {MinAmount} - {MaxAmount}", 
                amountRange.MinVal, amountRange.MaxVal);

            if (amountRange.MinVal.HasValue)
                expenses = expenses.Where(e => e.Amount >= amountRange.MinVal.Value).ToList();

            if (amountRange.MaxVal.HasValue)
                expenses = expenses.Where(e => e.Amount <= amountRange.MaxVal.Value).ToList();

            return expenses;
        }

        private List<Expense> FilterByDate(List<Expense> expenses, Range<DateTimeOffset>? dateRange)
        {
            if (dateRange == null) 
            {
                _logger.LogTrace("No date range filter applied");
                return expenses;
            }

            _logger.LogTrace("Filtering expenses by date range {StartDate} - {EndDate}", 
                dateRange.MinVal, dateRange.MaxVal);

            if (dateRange.MinVal.HasValue)
                expenses = expenses.Where(e => e.CreatedAt >= dateRange.MinVal.Value).ToList();

            if (dateRange.MaxVal.HasValue)
                expenses = expenses.Where(e => e.CreatedAt <= dateRange.MaxVal.Value).ToList();

            return expenses;
        }
    }
}