using BankApplication.Models.DTOs;

namespace BankApplication.Interfaces
{
    public interface IOtherContextFunctionalities
    {
        Task<List<SentBankTransactionResponseDto>> GetSentBankTransactionsByAccount(int account_id);
        Task<List<ReceivedBankTransactionResponseDto>> GetReceivedBankTransactionsByAccount(int account_id);
        Task<List<DepositResponseDto>> GetDepositsByAccount(int account_id);
        Task<List<WithdrawalResponseDto>> GetWithdrawalsByAccount(int account_id);
    }
}