using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackingSystem.Validation
{
    public class RoleValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            // Allow null values (no update requested)
            if (value == null)
                return true;

            // Allow empty strings (no update requested)
            if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                return true;

            // Validate actual role values
            if (value is string role)
            {
                return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                    role.Equals("User", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Role must be either 'Admin' or 'User', or null to keep current role.";
        }
    }
}
