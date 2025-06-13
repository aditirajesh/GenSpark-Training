using System.ComponentModel.DataAnnotations;
using ExpenseTrackingSystem.Validation;

namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ExpenseAddRequestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [TextValidation]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [TextValidation]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notes are required.")]
        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [AmountValidation]
        public decimal Amount { get; set; }

        [FileValidation]
        public IFormFile? ReceiptBill { get; set; }

        // NEW: Optional field for admins to specify target user
        [EmailValidation]
        public string? TargetUsername { get; set; } = null;
    }
}