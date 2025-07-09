using System.ComponentModel.DataAnnotations;
using ExpenseTrackingSystem.Validation;

namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ReceiptAddRequestDto
    {
        [Required(ErrorMessage = "Receipt file is required.")]
        [FileValidation]
        public IFormFile ReceiptBill { get; set; }

        [Required(ErrorMessage = "Receipt name is required.")]
        [TextValidation]
        public string ReceiptName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [EmailValidation]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [TextValidation]
        public string Category { get; set; } = string.Empty;
        public Guid ExpenseId { get; set; }
    }
}