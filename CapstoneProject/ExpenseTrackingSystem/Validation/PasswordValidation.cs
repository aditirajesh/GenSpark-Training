using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ExpenseTrackingSystem.Validation
{
    public class PasswordValidation : ValidationAttribute
    {
        public PasswordValidation()
        {
            ErrorMessage = "Password must be at least 6 characters long, and contain at least one uppercase letter, one number, and one special character.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return true;
            
            var password = value as string ?? "";
            return password.Length >= 6 &&
                Regex.IsMatch(password, @"[A-Z]") &&
                Regex.IsMatch(password, @"[0-9]") &&
                Regex.IsMatch(password, @"[\W_]");
        }
    }

}

