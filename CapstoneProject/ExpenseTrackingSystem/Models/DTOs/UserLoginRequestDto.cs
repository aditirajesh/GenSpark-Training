using System.ComponentModel.DataAnnotations;
using ExpenseTrackingSystem.Validation;

namespace ExpenseTrackingSystem.Models.DTOs
{
    public class UserLoginRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [EmailValidation]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [PasswordValidation] 
        public string Password { get; set; } = string.Empty;
    }
}