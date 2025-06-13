using System.ComponentModel.DataAnnotations;
using ExpenseTrackingSystem.Validation;

namespace ExpenseTrackingSystem.Models.DTOs
{
    public class UserAddRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [EmailValidation]
        public string Username { get; set; } = string.Empty;

        [PhoneValidation]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "User role is required.")]
        [RoleValidation]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [PasswordValidation]
        public string Password { get; set; } = string.Empty;
    }
}