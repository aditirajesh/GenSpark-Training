using System.Data.SqlTypes;
using FileHandlerApplication.Models;

namespace FileHandlerApplication.Interfaces
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
    }
}