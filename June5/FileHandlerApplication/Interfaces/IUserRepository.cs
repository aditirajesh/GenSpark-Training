using FileHandlerApplication.Models;

namespace FileHandlerApplication.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddUser(User user);
        Task<User> GetByUsername(string username);
        
    }
}