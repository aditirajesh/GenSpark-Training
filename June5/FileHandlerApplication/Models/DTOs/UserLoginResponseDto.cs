namespace FileHandlerApplication.Models.DTOs
{
    public class UserLoginResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}