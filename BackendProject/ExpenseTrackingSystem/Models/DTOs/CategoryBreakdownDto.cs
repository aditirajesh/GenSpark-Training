
namespace ExpenseTrackingSystem.Models.DTOs
{
   public class CategoryBreakdownDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
        public decimal Percentage { get; set; }
        public decimal AverageAmount { get; set; }
    } 
}
