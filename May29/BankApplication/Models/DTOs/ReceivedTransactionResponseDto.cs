namespace BankApplication.Models.DTOs
{
    public class ReceivedBankTransactionResponseDto
    {
        public ReceivedBankTransactionResponseDto() { }
        public int transaction_id { get; set; }
        public int sender_id { get; set; }
        public decimal amount { get; set; }
        public DateTime transfer_date { get; set; }
    }
}