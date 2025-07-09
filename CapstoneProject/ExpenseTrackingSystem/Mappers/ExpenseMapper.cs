using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Mappers
{
    public class ExpenseMapper
    {
        public Expense MapAddRequestExpense(ExpenseAddRequestDto dto)
        {
            var expense = new Expense();
            expense.Title = dto.Title;
            expense.Category = dto.Category;
            expense.Notes = dto.Notes;
            expense.Amount = dto.Amount;
            return expense;
        }
    }
}