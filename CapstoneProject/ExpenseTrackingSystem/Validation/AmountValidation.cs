using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackingSystem.Validation
{
    public class AmountValidation : ValidationAttribute
    {
        public AmountValidation()
            {
                ErrorMessage = "Amount must be a positive number greater than zero.";
            }

        public override bool IsValid(object? value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return true;
                    
                return value is decimal d && d > 0;
            }
    }


}
