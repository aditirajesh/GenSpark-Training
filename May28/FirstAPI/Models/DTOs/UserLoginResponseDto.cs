namespace FirstAPI.Models.DTOs
{
    public class UserLoginResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Token { get; set; }
        //token is returned to user after successful login
        //used to authenticate user in the future w/o login and password


    }
}