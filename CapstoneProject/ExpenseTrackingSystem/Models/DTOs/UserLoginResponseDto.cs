namespace ExpenseTrackingSystem.Models.DTOs
{
    public class UserLoginResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}