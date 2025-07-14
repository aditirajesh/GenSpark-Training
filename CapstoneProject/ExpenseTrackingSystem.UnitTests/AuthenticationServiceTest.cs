using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Text;

[TestFixture]
    public class AuthenticationServiceTest
    {
        private Mock<IEncryptionService> _encryptionServiceMock;
        private Mock<IRepository<string, User>> _userRepositoryMock;
        private Mock<ITokenService> _tokenServiceMock;
        private AuthenticationService _authService;
        private Mock<ILogger<AuthenticationService>> _mockLogger;
    

        [SetUp]
        public void Setup()
        {
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _userRepositoryMock = new Mock<IRepository<string, User>>();
            _tokenServiceMock = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthenticationService>>();
            _authService = new AuthenticationService(_encryptionServiceMock.Object, _userRepositoryMock.Object, _tokenServiceMock.Object,_mockLogger.Object);
        }

        [Test]
        public async Task LoginPassTest()
        {
            // Arrange
            var loginDto = new UserLoginRequestDto { Username = "testuser", Password = "password123" };
            var user = new User { Username = "testuser", Password = "hashedpassword" };
            
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);
            _encryptionServiceMock.Setup(e => e.VerifyData("password123", "hashedpassword")).Returns(true);
            _tokenServiceMock.Setup(t => t.GenerateToken(user)).ReturnsAsync("access_token");
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");

            // Act
            var result = await _authService.Login(loginDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("testuser"));
            Assert.That(result.AccessToken, Is.EqualTo("access_token"));
        }

        [Test]
        public void LoginUserFailTest_UserNotFound()
        {
            // Arrange
            var loginDto = new UserLoginRequestDto { Username = "nonexistent", Password = "password123" };
            _userRepositoryMock.Setup(r => r.GetByID("nonexistent")).ReturnsAsync((User)null);

            // Act & Assert
            Assert.ThrowsAsync<EntityNotFoundException>(async () => await _authService.Login(loginDto));
        }

        [Test]
        public void LoginUserFailTest_InvalidPassword()
        {
            // Arrange
            var loginDto = new UserLoginRequestDto { Username = "testuser", Password = "wrongpassword" };
            var user = new User { Username = "testuser", Password = "hashedpassword" };
            
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);
            _encryptionServiceMock.Setup(e => e.VerifyData("wrongpassword", "hashedpassword")).Returns(false);

            // Act & Assert
            Assert.ThrowsAsync<InvalidPasswordException>(async () => await _authService.Login(loginDto));
        }

        [Test]
        public async Task GetAccessTokenPassTest()
        {
            // Arrange
            var user = new User 
            { 
                Username = "testuser", 
                RefreshToken = "valid_refresh_token",
                RefreshTokenExpiryTime = DateTimeOffset.Now.AddDays(1)
            };
            
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateToken(user)).ReturnsAsync("new_access_token");
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new_refresh_token");

            // Act
            var result = await _authService.GetAccessToken("testuser", "valid_refresh_token");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.EqualTo("new_access_token"));
        }
    }
