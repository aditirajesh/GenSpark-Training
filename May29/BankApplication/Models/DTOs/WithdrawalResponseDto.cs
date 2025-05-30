namespace BankApplication.Models.DTOs
{
    public class WithdrawalResponseDto
    {
        public WithdrawalResponseDto() { }
        public int transaction_id { get; set; }
        public decimal amount { get; set; }
        public DateTime transfer_date { get; set; }
    }
}