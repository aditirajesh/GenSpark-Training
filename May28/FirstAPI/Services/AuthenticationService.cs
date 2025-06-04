using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<string, User> _userRepository;
        private readonly ITokenService _tokenservice;

        public AuthenticationService(ILogger<AuthenticationService> logger,
                                IEncryptionService encryptionService,
                                IRepository<string, User> userRepository,
                                ITokenService tokenService)
        {
            _logger = logger;
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _tokenservice = tokenService;                          
        }
        public async Task<UserLoginResponseDto> Login(UserLoginRequestDto dto)
        {
            var dbUser = await _userRepository.GetByID(dto.Username);
            if (dbUser == null)
            {
                _logger.LogCritical("User not found");
                throw new Exception("User not found");
            }
            var encryptedData = await _encryptionService.EncryptData(new EncryptModel()
            {
                Data = dto.Password,
                HashKey = dbUser.HashKey
            });

            if (!encryptedData.EncryptedData.SequenceEqual(dbUser.Password))
            {
                _logger.LogError("Invalid password");
                throw new Exception("Invalid password");
            }

            var token = await _tokenservice.GenerateToken(dbUser);
            return new UserLoginResponseDto()
            {
                Username = dbUser.Username,
                Token = token
            };

        }
    }
}