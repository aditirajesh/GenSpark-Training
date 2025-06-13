using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace ExpenseTrackingSystem.Validation
{
   public class PhoneValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            // Allow null values (no update requested)
            if (value == null)
                return true;
                
            // Allow empty strings (no update requested)  
            if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                return true;
                
            // Validate actual phone values
            if (value is string phone)
            {
                // Your phone validation logic here
                return phone.Length == 10 && phone.All(char.IsDigit);
            }
            
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Phone must be 10 digits, or null to keep current phone.";
        }
    }

}

