using BankApplication.Interfaces;
using BankApplication.Models;
using BankApplication.Repositories;
using BankApplication.Mappers;
using BankApplication.Models.DTOs;
using System.Collections;



namespace BankApplication.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<int, Account> _accountrepository;
        private readonly IRepository<int, User> _userrepository;
        UserMapper userMapper;

        public UserService(IRepository<int, Account> accountrepository,
                            IRepository<int, User> userrepository,
                            UserMapper usermapper)
        {
            _accountrepository = accountrepository;
            _userrepository = userrepository;
            userMapper = usermapper;
        }

        public async Task<User> CreateUser(UserAddRequestDto dto)
        {
            var user = userMapper.MapAddRequestUser(dto);
            user = await _userrepository.Add(user);
            if (user == null)
            {
                throw new Exception("Unable to add user");
            }
            return user;

        }

        public async Task<ICollection<Account>> GetAccounts(int user_id)
        {
            var user = await _userrepository.GetById(user_id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            if (user.Accounts == null)
            {
                throw new Exception("Accounts not attached to user");
            }
            return user.Accounts;
        }

        public async Task<User> GetUserByName(string name)
        {
            var users = await _userrepository.GetAll();
            users = users.ToList();
            if (users == null || !users.Any())
            {
                throw new Exception("No users in database");
            }
            var user = users.FirstOrDefault(u => u.Name == name);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }
    }
}