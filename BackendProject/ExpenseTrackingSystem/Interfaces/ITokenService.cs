
using ExpenseTrackingSystem.Models;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
        public string GenerateRefreshToken();
    }
}