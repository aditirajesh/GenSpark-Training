using BankApplication.Interfaces;
using BankApplication.Mappers;
using BankApplication.Misc;
using BankApplication.Models.DTOs;
using BankApplication.Models;

namespace BankApplication.Services
{
    public class BankTransactionService : IBankTransactionService
    {
        private readonly IRepository<int, BankTransaction> _BankTransactionrepository;
        private readonly IRepository<int, Account> _accountrepository;
        private readonly IOtherContextFunctionalities _otherfunctionalities;

        BankTransactionMapper BankTransactionMapper;

        public BankTransactionService(IRepository<int, BankTransaction> BankTransactionrepository,
                                    IOtherContextFunctionalities otherfunctionalities,
                                    IRepository<int, Account> accountrepository,
                                    BankTransactionMapper BankTransactionmapper)
        {
            _BankTransactionrepository = BankTransactionrepository;
            _accountrepository = accountrepository;
            _otherfunctionalities = otherfunctionalities;
            BankTransactionMapper = BankTransactionmapper;
        }

        public async Task<BankTransaction> CreateBankTransaction(BankTransactionAddRequestDto dto)
        {
            var BankTransaction = BankTransactionMapper.MapAddRequestBankTransaction(dto);
            BankTransaction = await _BankTransactionrepository.Add(BankTransaction);
            if (BankTransaction == null)
            {
                throw new Exception("BankTransaction could not be added.");
            }
            if (BankTransaction.SenderId == null && BankTransaction.ReceiverId == null)
            {
                throw new Exception("Both sender and receiver IDs cannot be null");
            }
            if (BankTransaction.SenderId != null)
            {
                var senderAccount = await _accountrepository.GetById(BankTransaction.SenderId.Value);
                if (senderAccount == null)
                {
                    throw new Exception($"Sender account with ID {BankTransaction.SenderId.Value} not found.");
                }

                senderAccount.SentBankTransactions ??= [];
                senderAccount.SentBankTransactions.Add(BankTransaction);
            }

            if (BankTransaction.ReceiverId != null)
            {
                var receiverAccount = await _accountrepository.GetById(BankTransaction.ReceiverId.Value);
                if (receiverAccount == null)
                {
                    throw new Exception($"Receiver account with ID {BankTransaction.ReceiverId.Value} not found.");
                }

                receiverAccount.ReceivedBankTransactions ??= new List<BankTransaction>();
                receiverAccount.ReceivedBankTransactions.Add(BankTransaction);
            }

            return BankTransaction;
        }



        public async Task<ICollection<DepositResponseDto>> GetDeposits(int account_id)
        {
            var result = await _otherfunctionalities.GetDepositsByAccount(account_id);
            return result;
        }

        public async Task<ICollection<ReceivedBankTransactionResponseDto>> GetReceivedBankTransactions(int account_id)
        {
            var result = await _otherfunctionalities.GetReceivedBankTransactionsByAccount(account_id);
            return result;
        }

        public async Task<ICollection<SentBankTransactionResponseDto>> GetSentBankTransactions(int account_id)
        {
            var result = await _otherfunctionalities.GetSentBankTransactionsByAccount(account_id);
            return result;
        }

        public async Task<ICollection<WithdrawalResponseDto>> GetWithdrawals(int account_id)
        {
            var result = await _otherfunctionalities.GetWithdrawalsByAccount(account_id);
            return result;
        }

        public async Task<BankTransaction> GetBankTransactionDetails(int BankTransaction_id)
        {
            var BankTransaction = await _BankTransactionrepository.GetById(BankTransaction_id);
            if (BankTransaction == null)
            {
                throw new Exception("BankTransaction not found");
            }
            return BankTransaction;
        }
    }
}