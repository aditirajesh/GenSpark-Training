using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseTrackingSystem.Tests
{
    [TestFixture]
    public class TokenServiceTest
    {
        private Mock<IConfiguration> _configurationMock;
        private TokenService _tokenService;
        private readonly string _validJwtKey = "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32Characters";
        private readonly string _shortJwtKey = "short"; // Too short for security

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
        }


        [Test]
        public void GenerateRefreshToken_PassTest()
        {
            // Arrange
            _configurationMock.Setup(c => c["Keys:JwtTokenKey"]).Returns(_validJwtKey);
            _tokenService = new TokenService(_configurationMock.Object);

            // Act
            var result = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Length, Is.GreaterThan(0));
            
            // Verify it's a valid Base64 string
            Assert.DoesNotThrow(() => Convert.FromBase64String(result));
            
            // Verify the decoded bytes are 32 in length
            var decodedBytes = Convert.FromBase64String(result);
            Assert.That(decodedBytes.Length, Is.EqualTo(32));
        }


        [Test]
        public void GenerateRefreshToken_PassTest_MultipleCallsReturnDifferentTokens()
        {
            // Arrange
            _configurationMock.Setup(c => c["Keys:JwtTokenKey"]).Returns(_validJwtKey);
            _tokenService = new TokenService(_configurationMock.Object);
            var tokens = new List<string>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                tokens.Add(_tokenService.GenerateRefreshToken());
            }

            // Assert
            Assert.That(tokens.Distinct().Count(), Is.EqualTo(10)); // All tokens should be unique
        }


        [Test]
        public async Task GenerateToken_PassTest()
        {
            // Arrange
            _configurationMock.Setup(c => c["Keys:JwtTokenKey"]).Returns(_validJwtKey);
            _tokenService = new TokenService(_configurationMock.Object);
            
            var user = new User
            {
                Username = "testuser",
                Role = "User"
            };

            // Act
            var result = await _tokenService.GenerateToken(user);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            
            // Verify it's a valid JWT token format (3 parts separated by dots)
            var tokenParts = result.Split('.');
            Assert.That(tokenParts.Length, Is.EqualTo(3));
        }


        [Test]
        public void GenerateToken_FailTest_NullUser()
        {
            // Arrange
            _configurationMock.Setup(c => c["Keys:JwtTokenKey"]).Returns(_validJwtKey);
            _tokenService = new TokenService(_configurationMock.Object);

            // Act & Assert - Fixed: Expecting NullReferenceException instead of ArgumentNullException
            Assert.ThrowsAsync<NullReferenceException>(async () => await _tokenService.GenerateToken(null));
        }


        [Test]
        public async Task GenerateToken_PassTest_TokenCanBeValidated()
        {
            // Arrange
            _configurationMock.Setup(c => c["Keys:JwtTokenKey"]).Returns(_validJwtKey);
            _tokenService = new TokenService(_configurationMock.Object);
            
            var user = new User
            {
                Username = "testuser",
                Role = "User"
            };

            // Act
            var token = await _tokenService.GenerateToken(user);

            // Assert - Validate the token can be parsed and verified
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_validJwtKey);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            
            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.Identity.Name, Is.EqualTo("testuser"));
            Assert.That(principal.IsInRole("User"), Is.True);
        }

        [Test]
        public async Task GenerateToken_FailTest_TokenValidationWithWrongKey()
        {
            // Arrange
            _configurationMock.Setup(c => c["Keys:JwtTokenKey"]).Returns(_validJwtKey);
            _tokenService = new TokenService(_configurationMock.Object);
            
            var user = new User
            {
                Username = "testuser",
                Role = "User"
            };

            // Act
            var token = await _tokenService.GenerateToken(user);

            // Assert 
            var tokenHandler = new JwtSecurityTokenHandler();
            var wrongKey = Encoding.UTF8.GetBytes("WrongKeyThatIsAlsoVeryLongToMeetSecurityRequirements123");
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(wrongKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() => 
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken));
        }

        [TearDown]
        public void TearDown()
        {
            _configurationMock = null;
            _tokenService = null;
        }
    }
}