using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ExpenseTrackingSystem.Validation
{
    public class EmailValidation : ValidationAttribute
    {
        public EmailValidation()
        {
            ErrorMessage = "Username must be a valid email address format (e.g., user@example.com).";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return true;
            
            var email = value as string ?? "";
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }

}

