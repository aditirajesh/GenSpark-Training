using System.Text;
using FileHandlerApplication.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using FileHandlerApplication.Models;
using System.Security.Claims;

namespace FileHandlerApplication.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _securityKey;

        public TokenService(IConfiguration configuration)
        {
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Keys:JWTTokenKey"]));
        }

        public async Task<string> GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var creds = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(2),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}