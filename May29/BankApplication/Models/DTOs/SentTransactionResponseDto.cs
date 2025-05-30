namespace BankApplication.Models.DTOs
{
    public class SentBankTransactionResponseDto
    {
        public SentBankTransactionResponseDto() { }
        public int transaction_id { get; set; }
        public int receiver_id { get; set; }
        public decimal amount { get; set; }
        public DateTime transfer_date { get; set; }
    }
}