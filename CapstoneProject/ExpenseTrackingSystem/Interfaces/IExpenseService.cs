using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IExpenseService
    {
        Task<Expense> AddExpense(ExpenseAddRequestDto dto, string targetUsername, string? createdByUsername = null);
        Task<Expense> UpdateExpense(ExpenseUpdateRequestDto dto, string username);
        Task<Expense> DeleteExpense(Guid id, string deletedBy);
        Task<Expense> GetExpense(Guid id);
        Task<ICollection<Expense>> SearchExpense(ExpenseSearchModel searchModel);
        Task<ICollection<Expense>> GetExpensesByUsername(string username, int pageNumber = 1, int pageSize = 10);
        Task<ICollection<Expense>> SearchUserExpenses(ExpenseSearchModel searchModel, string username);
        Task<ReceiptResponseDto> GetExpenseReceipt(Guid receiptId, string username);
    }
}