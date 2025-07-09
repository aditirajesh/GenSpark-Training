using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IReceiptService
    {
        Task<Receipt> CreateReceipt(ReceiptAddRequestDto dto);
        Task<Receipt> DeleteReceipt(Guid receipt_id);
        Task<Receipt> UpdateReceipt(ReceiptUpdateRequestDto dto, Guid receipt_id);
        Task<ReceiptResponseDto> GetReceipt(Guid receipt_id);
        Task<ICollection<Receipt>?> SearchReceipts(ReceiptSearchModel searchModel);


    }
}