using ExpenseTrackingSystem.Validation;

namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ReceiptUpdateRequestDto
    {
        [FileValidation]
        public IFormFile? Receipt { get; set; }

        [TextValidation]
        public string? Category { get; set; }

        [TextValidation]
        public string? ReceiptName { get; set; }
    }
}