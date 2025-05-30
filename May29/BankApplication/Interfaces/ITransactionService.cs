using BankApplication.Models.DTOs;
using BankApplication.Models;

namespace BankApplication.Interfaces
{
    public interface IBankTransactionService
    {
        Task<BankTransaction> CreateBankTransaction(BankTransactionAddRequestDto dto);
        Task<ICollection<SentBankTransactionResponseDto>> GetSentBankTransactions(int account_id);
        Task<ICollection<ReceivedBankTransactionResponseDto>> GetReceivedBankTransactions(int account_id);
        Task<ICollection<DepositResponseDto>> GetDeposits(int account_id);
        Task<ICollection<WithdrawalResponseDto>> GetWithdrawals(int account_id);
        Task<BankTransaction> GetBankTransactionDetails(int BankTransaction_id);
    }
}