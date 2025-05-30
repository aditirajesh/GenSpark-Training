namespace BankApplication.Models
{
    public class BankTransaction
    {
        public int BankTransactionId { get; set; }
        public int? SenderId { get; set; } //nullable for withdrawal
        public int? ReceiverId { get; set; } // nullable for deposit
        public decimal AmountTransferred { get; set; }
        public DateTime TransferDate { get; set; }
        public Account? Sender { get; set; }
        public Account? Receiver { get; set; }
    }
}