using ExpenseTrackingSystem.Validation;

namespace ExpenseTrackingSystem.Models.DTOs
{
    public class ExpenseUpdateRequestDto
    {
        public Guid Id { get; set; }

        [TextValidation]
        public string? Title { get; set; } = null;

        [TextValidation]
        public string? Category { get; set; } = null;

        [TextValidation]
        public string? Notes { get; set; } = null;

        [AmountValidation]
        public decimal? Amount { get; set; } = null;

        [FileValidation]
        public IFormFile? Receipt { get; set; } = null;

        [ExpenseDateValidation]
        public DateTimeOffset? ExpenseDate { get; set; } = null;
    }
}