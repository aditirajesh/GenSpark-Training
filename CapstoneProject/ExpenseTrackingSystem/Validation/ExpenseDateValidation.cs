using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackingSystem.Validation
{
    public class ExpenseDateValidation: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset <= DateTimeOffset.UtcNow;
            }

            if (value is DateTime dateTime)
            {
                var dateTimeOffsetValue = new DateTimeOffset(dateTime);
                return dateTimeOffsetValue <= DateTimeOffset.UtcNow;
            }

            // Invalid type
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field cannot be a future date. Please select a past or current date.";
        }
    }
}