namespace BankApplication.Models.DTOs
{
    public class BankTransactionAddRequestDto
    {
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public decimal AmountTransferred { get; set; }
    }
}