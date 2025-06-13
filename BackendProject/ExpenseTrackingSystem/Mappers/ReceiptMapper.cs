using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Mappers
{
    public class ReceiptMapper
    {
        public Receipt MapAddRequestReceipt(ReceiptAddRequestDto dto)
        {
            var receipt = new Receipt();
            receipt.ReceiptName = dto.ReceiptName;
            receipt.Username = dto.Username;
            receipt.Category = dto.Category;
            receipt.ExpenseId = dto.ExpenseId;
            return receipt;
        }
    }
}