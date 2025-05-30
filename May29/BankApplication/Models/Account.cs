namespace BankApplication.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
        public ICollection<BankTransaction>? SentBankTransactions { get; set; }
        public ICollection<BankTransaction>? ReceivedBankTransactions { get; set; }


    }
}