using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ExpenseTrackingSystem.Tests
{
    [TestFixture]
    public class UserServiceTest
    {
        private Mock<IEncryptionService> _encryptionServiceMock;
        private Mock<IRepository<string, User>> _userRepositoryMock;
        private Mock<IAuditLogService> _auditLogServiceMock;
        private Mock<IRepository<Guid, Expense>> _expenseRepositoryMock;
        private Mock<IReceiptService> _receiptServiceMock;
        private Mock<IRepository<Guid, Receipt>> _receiptRepositoryMock;
        private UserService _userService;
        private Mock<ILogger<UserService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _userRepositoryMock = new Mock<IRepository<string, User>>();
            _auditLogServiceMock = new Mock<IAuditLogService>();
            _expenseRepositoryMock = new Mock<IRepository<Guid, Expense>>();
            _receiptServiceMock = new Mock<IReceiptService>();
            _receiptRepositoryMock = new Mock<IRepository<Guid, Receipt>>();
            _mockLogger = new Mock<ILogger<UserService>>();
            
            // ✅ Updated constructor to match new UserService signature
            _userService = new UserService(
                _encryptionServiceMock.Object,
                _userRepositoryMock.Object,
                _auditLogServiceMock.Object,
                _expenseRepositoryMock.Object,
                _receiptServiceMock.Object,
                _receiptRepositoryMock.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task CreateUserPassTest()
        {
            // Arrange
            var dto = new UserAddRequestDto
            {
                Username = "testuser",
                Password = "password123",
                Role = "User",
                Phone = "1234567890"
            };
            var user = new User 
            { 
                Username = "testuser", 
                Role = "User", 
                Phone = "1234567890",
                IsDeleted = false
            };
            var encryptModel = new EncryptModel { EncryptedData = "hashedpassword" };

            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync((User)null);
            _encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>())).ReturnsAsync(encryptModel);
            _userRepositoryMock.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync(user);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _userService.CreateUser(dto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("testuser"));
            Assert.That(result.Role, Is.EqualTo("User"));
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void CreateUserFailTest_DuplicateUsername()
        {
            // Arrange
            var dto = new UserAddRequestDto { Username = "existinguser", Password = "password", Role = "User" };
            var existingUser = new User { Username = "existinguser", IsDeleted = false };
            
            _userRepositoryMock.Setup(r => r.GetByID("existinguser")).ReturnsAsync(existingUser);

            // Act & Assert
            var ex = Assert.ThrowsAsync<DuplicateEntityException>(async () => 
                await _userService.CreateUser(dto));
            Assert.That(ex.Message, Is.EqualTo("Username already taken"));
        }

        [Test]
        public async Task CreateUserPassTest_ReactivateDeletedUser()
        {
            // Arrange
            var dto = new UserAddRequestDto
            {
                Username = "deleteduser",
                Password = "newpassword",
                Role = "Admin",
                Phone = "9876543210"
            };
            var deletedUser = new User 
            { 
                Username = "deleteduser", 
                IsDeleted = true,
                Role = "User",
                Phone = "1111111111"
            };
            var encryptModel = new EncryptModel { EncryptedData = "newhashedpassword" };

            _userRepositoryMock.Setup(r => r.GetByID("deleteduser")).ReturnsAsync(deletedUser);
            _encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>())).ReturnsAsync(encryptModel);
            _userRepositoryMock.Setup(r => r.Update("deleteduser", It.IsAny<User>())).ReturnsAsync(deletedUser);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _userService.CreateUser(dto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("deleteduser"));
            Assert.That(result.IsDeleted, Is.False);
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public async Task DeleteUserPassTest()
        {
            // Arrange
            var user = new User 
            { 
                Username = "testuser", 
                IsDeleted = false,
                Role = "User",
                Expenses = new List<Expense>(), // ✅ Added for expense count
                Receipts = new List<Receipt>()  // ✅ Added for receipt count
            };

            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.Update("testuser", It.IsAny<User>())).ReturnsAsync(user);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act - ✅ Updated to match new method signature with deletedBy parameter
            var result = await _userService.DeleteUser("testuser", "admin");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("testuser"));
            Assert.That(result.IsDeleted, Is.True);
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void DeleteUserFailTest_UserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByID("nonexistent")).ReturnsAsync((User)null);

            // Act & Assert - ✅ Updated to match new method signature
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _userService.DeleteUser("nonexistent", "admin"));
            Assert.That(ex.Message, Is.EqualTo("User not found in database"));
        }

        [Test]
        public void DeleteUserFailTest_UserAlreadyDeleted()
        {
            // Arrange
            var deletedUser = new User { Username = "deleteduser", IsDeleted = true };
            _userRepositoryMock.Setup(r => r.GetByID("deleteduser")).ReturnsAsync(deletedUser);

            // Act & Assert - ✅ Added test for already deleted user
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _userService.DeleteUser("deleteduser", "admin"));
            Assert.That(ex.Message, Is.EqualTo("User is already deleted"));
        }

        [Test]
        public async Task GetUserByUsernamePassTest()
        {
            // Arrange
            var user = new User 
            { 
                Username = "testuser", 
                IsDeleted = false,
                Role = "User",
                Phone = "1234567890"
            };
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByUsername("testuser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("testuser"));
            Assert.That(result.IsDeleted, Is.False);
        }

        [Test]
        public void GetUserByUsernameFailTest_UserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByID("nonexistent")).ReturnsAsync((User)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _userService.GetUserByUsername("nonexistent"));
            Assert.That(ex.Message, Is.EqualTo("Could not find user"));
        }

        [Test]
        public void GetUserByUsernameFailTest_UserIsDeleted()
        {
            // Arrange
            var deletedUser = new User { Username = "deleteduser", IsDeleted = true };
            _userRepositoryMock.Setup(r => r.GetByID("deleteduser")).ReturnsAsync(deletedUser);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _userService.GetUserByUsername("deleteduser"));
            Assert.That(ex.Message, Is.EqualTo("Could not find user"));
        }

        [Test]
        public async Task GetAllUsersPassTest()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Username = "user1", IsDeleted = false, Role = "User" },
                new User { Username = "user2", IsDeleted = false, Role = "Admin" }
            };
            _userRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(users);

            // Act
            var result = await _userService.GetAllUsers();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(u => u.Username == "user1"), Is.True);
            Assert.That(result.Any(u => u.Username == "user2"), Is.True);
        }

        [Test]
        public void GetAllUsersFailTest_NoUsers()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(new List<User>());

            // Act & Assert
            var ex = Assert.ThrowsAsync<CollectionEmptyException>(async () => 
                await _userService.GetAllUsers());
            Assert.That(ex.Message, Is.EqualTo("No users present"));
        }

        [Test]
        public void GetAllUsersFailTest_NullUsers()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync((List<User>)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<CollectionEmptyException>(async () => 
                await _userService.GetAllUsers());
            Assert.That(ex.Message, Is.EqualTo("No users present"));
        }

        [Test]
        public async Task UpdateUserDetailsPassTest()
        {
            // Arrange
            var existingUser = new User 
            { 
                Username = "testuser", 
                Phone = "1111111111", 
                Role = "User",
                IsDeleted = false 
            };
            var updateDto = new UserUpdateRequestDto 
            { 
                Phone = "2222222222", 
                Role = "Admin",
                Password = "newpassword"
            };
            var encryptModel = new EncryptModel { EncryptedData = "newhashedpassword" };

            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(existingUser);
            _encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>())).ReturnsAsync(encryptModel);
            _userRepositoryMock.Setup(r => r.Update("testuser", It.IsAny<User>())).ReturnsAsync(existingUser);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act - ✅ Updated to match new method signature with additional parameters
            var result = await _userService.UpdateUserDetails("testuser", updateDto, true, false);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("testuser"));
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void UpdateUserDetailsFailTest_UserNotFound()
        {
            // Arrange
            var updateDto = new UserUpdateRequestDto { Phone = "2222222222" };
            _userRepositoryMock.Setup(r => r.GetByID("nonexistent")).ReturnsAsync((User)null);

            // Act & Assert - ✅ Updated method signature
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _userService.UpdateUserDetails("nonexistent", updateDto, false, false));
            Assert.That(ex.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public void UpdateUserDetailsFailTest_UserIsDeleted()
        {
            // Arrange
            var deletedUser = new User { Username = "deleteduser", IsDeleted = true };
            var updateDto = new UserUpdateRequestDto { Phone = "2222222222" };
            
            _userRepositoryMock.Setup(r => r.GetByID("deleteduser")).ReturnsAsync(deletedUser);

            // Act & Assert - ✅ Updated method signature
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _userService.UpdateUserDetails("deleteduser", updateDto, false, false));
            Assert.That(ex.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public void UpdateUserDetailsFailTest_NonAdminTryingToChangeRole()
        {
            // Arrange
            var existingUser = new User 
            { 
                Username = "testuser", 
                Role = "User",
                IsDeleted = false 
            };
            var updateDto = new UserUpdateRequestDto { Role = "Admin" };
            
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(existingUser);

            // Act & Assert - ✅ Test non-admin trying to change role
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () => 
                await _userService.UpdateUserDetails("testuser", updateDto, false, false));
            Assert.That(ex.Message, Is.EqualTo("Only administrators can change user roles"));
        }

        [Test]
        public void UpdateUserDetailsFailTest_AdminChangingOwnRole()
        {
            // Arrange
            var existingUser = new User 
            { 
                Username = "admin", 
                Role = "Admin",
                IsDeleted = false 
            };
            var updateDto = new UserUpdateRequestDto { Role = "User" };
            
            _userRepositoryMock.Setup(r => r.GetByID("admin")).ReturnsAsync(existingUser);

            // Act & Assert - ✅ Test admin trying to change own role
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _userService.UpdateUserDetails("admin", updateDto, true, true));
            Assert.That(ex.Message, Is.EqualTo("Administrators cannot change their own role"));
        }

        [Test]
        public void UpdateUserDetailsFailTest_NoFieldsProvided()
        {
            // Arrange
            var existingUser = new User 
            { 
                Username = "testuser", 
                IsDeleted = false 
            };
            var updateDto = new UserUpdateRequestDto(); // No fields provided
            
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(existingUser);

            // Act & Assert - ✅ Test no fields provided
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _userService.UpdateUserDetails("testuser", updateDto, false, false));
            Assert.That(ex.Message, Is.EqualTo("At least one field (password, phone, or role) must be provided for update"));
        }

        [Test]
        public async Task SearchUsersPassTest()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Username = "admin1", Role = "Admin", IsDeleted = false, CreatedAt = DateTimeOffset.Now.AddDays(-1) },
                new User { Username = "user1", Role = "User", IsDeleted = false, CreatedAt = DateTimeOffset.Now },
                new User { Username = "deleteduser", Role = "User", IsDeleted = true, CreatedAt = DateTimeOffset.Now }
            };
            var searchModel = new UserSearchModel { Role = "Admin" };
            
            _userRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(users);

            // Act
            var result = await _userService.SearchUsers(searchModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Role, Is.EqualTo("Admin"));
            Assert.That(result.First().Username, Is.EqualTo("admin1"));
            // Verify deleted users are excluded
            Assert.That(result.Any(u => u.IsDeleted), Is.False);
        }

        [Test]
        public async Task SearchUsersFailTest_NoMatches()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Username = "user1", Role = "User", IsDeleted = false, CreatedAt = DateTimeOffset.Now },
                new User { Username = "user2", Role = "User", IsDeleted = false, CreatedAt = DateTimeOffset.Now }
            };
            var searchModel = new UserSearchModel { Role = "Manager" }; // No matches
            
            _userRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(users);

            // Act
            var result = await _userService.SearchUsers(searchModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task SearchUsersMultipleFiltersPassTest()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Username = "admin_john", Role = "Admin", IsDeleted = false, CreatedAt = DateTimeOffset.Now },
                new User { Username = "admin_jane", Role = "Admin", IsDeleted = false, CreatedAt = DateTimeOffset.Now },
                new User { Username = "user_john", Role = "User", IsDeleted = false, CreatedAt = DateTimeOffset.Now }
            };
            var searchModel = new UserSearchModel 
            { 
                Username = "admin",
                Role = "Admin"
            };
            
            _userRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(users);

            // Act
            var result = await _userService.SearchUsers(searchModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(u => u.Username.Contains("admin") && u.Role == "Admin"), Is.True);
        }

        [Test]
        public async Task SearchUsersNoFiltersPassTest()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Username = "user1", Role = "User", IsDeleted = false, CreatedAt = DateTimeOffset.Now.AddDays(-1) },
                new User { Username = "admin1", Role = "Admin", IsDeleted = false, CreatedAt = DateTimeOffset.Now },
                new User { Username = "deleteduser", Role = "User", IsDeleted = true, CreatedAt = DateTimeOffset.Now }
            };
            var searchModel = new UserSearchModel(); // No filters
            
            _userRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(users);

            // Act
            var result = await _userService.SearchUsers(searchModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2)); // Excludes deleted user
            Assert.That(result.All(u => !u.IsDeleted), Is.True);
            // Should be ordered by CreatedAt descending
            Assert.That(result.First().Username, Is.EqualTo("admin1"));
        }

        [TearDown]
        public void TearDown()
        {
            _encryptionServiceMock = null;
            _userRepositoryMock = null;
            _auditLogServiceMock = null;
            _expenseRepositoryMock = null;
            _receiptServiceMock = null;
            _receiptRepositoryMock = null;
            _userService = null;
        }
    }
}