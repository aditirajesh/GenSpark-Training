using BankApplication.Contexts;
using BankApplication.Interfaces;
using BankApplication.Models.DTOs;

namespace BankApplication.Misc
{
    public class OtherFunctionalitiesImplementation : IOtherContextFunctionalities
    {
        private readonly BankingContext _bankingcontext;
        public OtherFunctionalitiesImplementation(BankingContext bankingContext)
        {
            _bankingcontext = bankingContext;
        }
        public async Task<List<SentBankTransactionResponseDto>> GetSentBankTransactionsByAccount(int account_id)
        {
            var result = await _bankingcontext.GetSentBankTransactionsByAccount(account_id);
            return result;
        }

        public async Task<List<ReceivedBankTransactionResponseDto>> GetReceivedBankTransactionsByAccount(int account_id)
        {
            var result = await _bankingcontext.GetReceivedBankTransactionsByAccount(account_id);
            return result;
        }

        public async Task<List<DepositResponseDto>> GetDepositsByAccount(int account_id)
        {
            var result = await _bankingcontext.GetDepositsByAccount(account_id);
            return result;
        }

        public async Task<List<WithdrawalResponseDto>> GetWithdrawalsByAccount(int account_id)
        {
            var result = await _bankingcontext.GetWithdrawalsByAccount(account_id);
            return result;
        }
    }
}