using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IRepository<Guid, Expense> _expenseRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly ExpenseMapper _expenseMapper;
        private readonly IReceiptService _receiptService;
        private readonly IAuditLogService _auditLogService;

        public ExpenseService(
            IRepository<Guid, Expense> expenseRepository,
            IRepository<string, User> userRepository,
            IReceiptService receiptService,
            IAuditLogService auditLogService)
        {
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _expenseMapper = new();
            _receiptService = receiptService;
            _auditLogService = auditLogService;

        }

        public async Task<Expense> AddExpense(ExpenseAddRequestDto dto, string targetUsername, string? createdByUsername = null)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto), "Expense DTO cannot be null");

            var user = await _userRepository.GetByID(targetUsername)
                    ?? throw new EntityNotFoundException("Target user not found");

            var expense = _expenseMapper.MapAddRequestExpense(dto);

            expense.Username = targetUsername;
            expense.User = user;
            expense.CreatedAt = DateTimeOffset.UtcNow;
            expense.CreatedBy = createdByUsername ?? targetUsername;
            expense.UpdatedAt = DateTimeOffset.UtcNow;
            expense.UpdatedBy = createdByUsername ?? targetUsername;

            var addedExpense = await _expenseRepository.Add(expense)
                                ?? throw new EntityCreationException("Failed to add expense");

            user.Expenses ??= new List<Expense>();
            user.Expenses.Add(addedExpense);
            await _userRepository.Update(targetUsername, user);

            if (dto.ReceiptBill != null)
            {
                var receipt_dto = new ReceiptAddRequestDto
                {
                    ReceiptBill = dto.ReceiptBill,
                    ReceiptName = $"{targetUsername}_{addedExpense.Id}_{dto.Title}_receipt",
                    Username = targetUsername,
                    Category = dto.Category,
                    ExpenseId = addedExpense.Id
                };
                await _receiptService.CreateReceipt(receipt_dto);
            }

            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = createdByUsername ?? targetUsername,
                Action = "Create",
                EntityName = "Expense",
                Details = $"Created Expense '{addedExpense.Title}' (ID: {addedExpense.Id}) for user '{targetUsername}' with amount {addedExpense.Amount}",
                Timestamp = DateTimeOffset.UtcNow
            });
            return addedExpense;
        }

        public async Task<Expense> UpdateExpense(ExpenseUpdateRequestDto dto, string username)
        {
            var expense = await _expenseRepository.GetByID(dto.Id)
                        ?? throw new EntityNotFoundException("Expense not found");

            expense.Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : expense.Category;
            expense.Notes = !string.IsNullOrWhiteSpace(dto.Notes) ? dto.Notes : expense.Notes;
            expense.Amount = dto.Amount ?? expense.Amount;

            if (!string.IsNullOrWhiteSpace(dto.Title))
                expense.Title = dto.Title;
            
            expense.UpdatedAt = DateTimeOffset.UtcNow;
            expense.UpdatedBy = username;

            if (dto.Receipt != null)
            {
                var receipt_update_dto = new ReceiptUpdateRequestDto
                {
                    Receipt = dto.Receipt,
                    Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : null,
                    ReceiptName = !string.IsNullOrWhiteSpace(dto.Title) ? $"{username}_{expense.Id}_{dto.Title}_receipt" : null
                };
                
                // Check if expense has a receipt (navigation property)
                if (expense.Receipt != null)
                {
                    var receipt_update = await _receiptService.UpdateReceipt(receipt_update_dto, expense.Receipt.Id)
                                        ?? throw new EntityUpdateException("Could not update Receipt for this Expense");
                }
                else
                {
                    var receipt_add_dto = new ReceiptAddRequestDto
                    {
                        ReceiptBill = dto.Receipt,
                        Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : null,
                        ReceiptName = !string.IsNullOrWhiteSpace(dto.Title) ? $"{username}_{expense.Id}_{dto.Title}_receipt" : $"{username}_{expense.Id}_{expense.Title}",
                        Username = username,
                        ExpenseId = expense.Id
                    };
                    var receipt_add = await _receiptService.CreateReceipt(receipt_add_dto)
                                    ?? throw new EntityCreationException("Could not create Receipt for this Expense");
                }
            }

            var updatedExpense = await _expenseRepository.Update(expense.Id, expense)
                                ?? throw new EntityUpdateException("Could not update expense");

            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = username,
                Action = "Update",
                EntityName = "Expense",
                Details = "Expense Updated",
                Timestamp = DateTimeOffset.UtcNow
            });
            return updatedExpense;
        }

        public async Task<Expense> DeleteExpense(Guid id, string currentUsername)
        {
            var expense = await _expenseRepository.GetByID(id)
                        ?? throw new EntityNotFoundException("Expense not found");

            var originalTitle = expense.Title;
            var originalUsername = expense.Username;


            // Log audit
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = currentUsername,
                Action = "Delete",
                EntityName = "Expense",
                Details = $"Hard deleted Expense '{originalTitle}' (ID: {id}) owned by '{originalUsername}'",
                Timestamp = DateTimeOffset.UtcNow
            });

            // Delete expense - receipt will cascade delete automatically
            var deleted_expense = await _expenseRepository.Delete(id);
            Console.WriteLine($"Deleted Expense: {deleted_expense.Id} - {deleted_expense.Title}");
            
            return deleted_expense;
        }


        public async Task<Expense> GetExpense(Guid id)
        {
            return await _expenseRepository.GetByID(id)
                   ?? throw new EntityNotFoundException("Expense not found");
        }

        public async Task<ICollection<Expense>> SearchExpense(ExpenseSearchModel searchModel)
        {
            var expenses = (await _expenseRepository.GetAll()).ToList();

            expenses = FilterByTitle(expenses, searchModel.Title);
            expenses = FilterByCategory(expenses, searchModel.Category);
            expenses = FilterByAmount(expenses, searchModel.amountRange);
            expenses = FilterByDate(expenses, searchModel.dateRange);

            return expenses.OrderByDescending(e => e.CreatedAt).ToList();
        }

        private List<Expense> FilterByTitle(List<Expense> expenses, string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return expenses;
            return expenses.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<Expense> FilterByCategory(List<Expense> expenses, string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return expenses;
            return expenses.Where(e => e.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<Expense> FilterByAmount(List<Expense> expenses, Range<decimal>? amountRange)
        {
            if (amountRange == null) return expenses;

            if (amountRange.MinVal.HasValue)
                expenses = expenses.Where(e => e.Amount >= amountRange.MinVal.Value).ToList();

            if (amountRange.MaxVal.HasValue)
                expenses = expenses.Where(e => e.Amount <= amountRange.MaxVal.Value).ToList();

            return expenses;
        }

        private List<Expense> FilterByDate(List<Expense> expenses, Range<DateTimeOffset>? dateRange)
        {
            if (dateRange == null) return expenses;

            if (dateRange.MinVal.HasValue)
                expenses = expenses.Where(e => e.CreatedAt >= dateRange.MinVal.Value).ToList();

            if (dateRange.MaxVal.HasValue)
                expenses = expenses.Where(e => e.CreatedAt <= dateRange.MaxVal.Value).ToList();

            return expenses;
        }

    }
}
