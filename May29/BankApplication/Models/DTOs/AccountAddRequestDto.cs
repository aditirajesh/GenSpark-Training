namespace BankApplication.Models.DTOs
{
    public class AccountAddRequestDto
    {
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Balance { get; set; }

    }
}