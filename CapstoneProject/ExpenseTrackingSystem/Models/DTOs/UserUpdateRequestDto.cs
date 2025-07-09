using ExpenseTrackingSystem.Validation;

namespace ExpenseTrackingSystem.Models.DTOs
{
    public class UserUpdateRequestDto
    {
        [PhoneValidation]
        public string? Phone { get; set; } = null;

        [RoleValidation]
        public string? Role { get; set; } = null;

        [PasswordValidation]
        public string? Password { get; set; } = null;
        
    }
}