using System.ComponentModel.DataAnnotations;

namespace FirstAPI.Models.DTOs
{
    public class UserLoginRequestDto
    {
        [Required(ErrorMessage = "Username is Mandatory")] 
        [MinLength(5,ErrorMessage = "Invalid entry for username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is Mandatory")] 
        public string Password { get; set; } = string.Empty;
    }
}