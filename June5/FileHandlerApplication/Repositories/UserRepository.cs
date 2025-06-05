using FileHandlerApplication.Interfaces;
using FileHandlerApplication.Models;

namespace FileHandlerApplication.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users = new();

        public Task<User> AddUser(User user)
        {
            if (user == null)
            {
                throw new Exception("User cannot be null");
            }
            _users[user.Username] = user;
            return Task.FromResult(user);
        }

        public Task<User> GetByUsername(string username)
        {
            _users.TryGetValue(username, out var user);
            return Task.FromResult(user);
        }

    }
}