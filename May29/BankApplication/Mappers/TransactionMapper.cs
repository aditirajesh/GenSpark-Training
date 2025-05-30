using BankApplication.Models;
using BankApplication.Models.DTOs;

namespace BankApplication.Mappers
{
    public class BankTransactionMapper
    {
        public BankTransaction? MapAddRequestBankTransaction(BankTransactionAddRequestDto dto)
        {
            BankTransaction BankTransaction = new();
            BankTransaction.SenderId = dto.SenderId;
            BankTransaction.ReceiverId = dto.ReceiverId;
            BankTransaction.AmountTransferred = dto.AmountTransferred;
            return BankTransaction;
        }
    }
}