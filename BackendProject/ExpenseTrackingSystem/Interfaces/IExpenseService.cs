using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IExpenseService
    {
        Task<Expense> AddExpense(ExpenseAddRequestDto dto, string targetUsername, string? createdByUsername = null);
        Task<Expense> UpdateExpense(ExpenseUpdateRequestDto dto, string username);
        Task<Expense> DeleteExpense(Guid id,string deletedBy);
        Task<Expense> GetExpense(Guid id);
        Task<ICollection<Expense>> SearchExpense(ExpenseSearchModel searchModel);
    }
}