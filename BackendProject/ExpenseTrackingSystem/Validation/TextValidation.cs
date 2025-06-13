using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace ExpenseTrackingSystem.Validation
{
    public class TextValidation : ValidationAttribute
    {
        public TextValidation()
        {
            ErrorMessage = "Field must contain at least one letter or number and cannot be blank or made up of only symbols.";
        }

        public override bool IsValid(object? value)
        {
            // Allow null or empty values - let [Required] handle that validation
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true;

            var text = value.ToString()!;
            
            // Check if the text contains at least one alphanumeric character
            // This allows spaces, punctuation, but requires at least one letter or number
            return Regex.IsMatch(text, @".*[a-zA-Z0-9].*");
        }
    }
}

