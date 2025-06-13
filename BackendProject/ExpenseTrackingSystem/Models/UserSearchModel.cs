
namespace ExpenseTrackingSystem.Models
{
    public class UserSearchModel
    {
        public string? Username { get; set; }
        public string? Role { get; set; }
        public Range<DateTimeOffset>? CreatedAtRange { get; set; }
    }
}