using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace ExpenseTrackingSystem.Validation
{
    public class FileValidation : ValidationAttribute
    {
        public FileValidation()
        {
            ErrorMessage = "Only PDF, PNG, JPG, or JPEG file formats are allowed.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return true;
            var file = value as IFormFile;
            if (file == null) return true;

            var allowedTypes = new[] { ".pdf", ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            return !string.IsNullOrEmpty(ext) && allowedTypes.Contains(ext);
        }
    }

}

