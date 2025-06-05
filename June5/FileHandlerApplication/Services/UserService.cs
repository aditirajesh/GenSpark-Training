using FileHandlerApplication.Interfaces;
using FileHandlerApplication.Mappers;
using FileHandlerApplication.Models;
using FileHandlerApplication.Models.DTOs;
using FileHandlerApplication.Repositories;

namespace FileHandlerApplication.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userrepository;
        private readonly IEncryptionService _encryptionservice;
        private readonly UserMapper _userMapper;
        private readonly ITokenService _tokenservice;

        public UserService(IUserRepository repo,
                        IEncryptionService encryptionService,
                        ITokenService tokenService
                        )
        {
            _userrepository = repo;
            _encryptionservice = encryptionService;
            _userMapper = new();
            _tokenservice = tokenService;
        }

        public async Task<User> CreateUser(UserAddRequestDto dto)
        {
            if (dto == null)
            {
                throw new Exception("User cannot be null");
            }
            var existingUser = await _userrepository.GetByUsername(dto.Username); 
            if (existingUser == null)
            {
                var user = _userMapper.MapRequestToUser(dto);
                var encryptedData = await _encryptionservice.EncryptData(new EncryptModel
                {
                    Data = dto.Password //encrypting the password
                });
                user.Password = encryptedData.EncryptedData;
                user.HashKey = encryptedData.HashKey;

                user = await _userrepository.AddUser(user);
                return user;
            }
            else
            {
                throw new Exception("Username already present in the database");
            }
        }

        public async Task<UserLoginResponseDto> Login(UserLoginRequestDto dto)
        {
            var user = await _userrepository.GetByUsername(dto.Username);
            if (user == null)
            {
                throw new Exception("user not found");
            }
            var encryptedData = await _encryptionservice.EncryptData(new EncryptModel()
            {
                Data = dto.Password,
                HashKey = user.HashKey
            });

            if (!encryptedData.EncryptedData.SequenceEqual(user.Password))
            {
                throw new Exception("Invalid password");
            }

            var token = await _tokenservice.GenerateToken(user);
            return new UserLoginResponseDto()
            {
                Username = user.Username,
                Token = token
            };
        }
    }
}