using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackingSystem.Models
{
    public class Expense
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Notes  { get; set; } = string.Empty;
        
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public Receipt? Receipt { get; set; }
        public User? User { get; set; }

    }
}