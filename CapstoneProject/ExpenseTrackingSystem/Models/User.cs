namespace ExpenseTrackingSystem.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
        public ICollection<Expense>? Expenses { get; set; }
        public ICollection<Receipt>? Receipts { get; set; }
    }
}