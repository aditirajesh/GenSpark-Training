namespace ExpenseTrackingSystem.Models.DTOs
{
    public class TokenRefreshRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}