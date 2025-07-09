using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ExpenseTrackingSystem.Tests
{
    [TestFixture]
    public class ExpenseServiceTest
    {
        private Mock<IRepository<Guid, Expense>> _expenseRepositoryMock;
        private Mock<IRepository<string, User>> _userRepositoryMock;
        private Mock<IReceiptService> _receiptServiceMock;
        private Mock<IAuditLogService> _auditLogServiceMock;
        private ExpenseService _expenseService;
        private Mock<ILogger<ExpenseService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _expenseRepositoryMock = new Mock<IRepository<Guid, Expense>>();
            _userRepositoryMock = new Mock<IRepository<string, User>>();
            _receiptServiceMock = new Mock<IReceiptService>();
            _auditLogServiceMock = new Mock<IAuditLogService>();
            _mockLogger = new Mock<ILogger<ExpenseService>>();
            
            _expenseService = new ExpenseService(
                _expenseRepositoryMock.Object,
                _userRepositoryMock.Object,
                _receiptServiceMock.Object,
                _auditLogServiceMock.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task AddExpensePassTest()
        {
            // Arrange
            var dto = new ExpenseAddRequestDto
            {
                Title = "Test Expense",
                Amount = 100.50m,
                Category = "Food",
                Notes = "Test notes"
            };
            var user = new User { Username = "testuser", Expenses = new List<Expense>() };
            var expense = new Expense 
            { 
                Id = Guid.NewGuid(), 
                Title = "Test Expense",
                Amount = 100.50m,
                Category = "Food",
                Username = "testuser"
            };

            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);
            _expenseRepositoryMock.Setup(r => r.Add(It.IsAny<Expense>())).ReturnsAsync(expense);
            _userRepositoryMock.Setup(r => r.Update("testuser", It.IsAny<User>())).ReturnsAsync(user);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _expenseService.AddExpense(dto, "testuser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Test Expense"));
            Assert.That(result.Amount, Is.EqualTo(100.50m));
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void AddExpenseFailTest_NullDto()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _expenseService.AddExpense(null, "testuser"));
            Assert.That(ex.ParamName, Is.EqualTo("dto"));
        }

        [Test]
        public void AddExpenseFailTest_UserNotFound()
        {
            // Arrange
            var dto = new ExpenseAddRequestDto { Title = "Test", Amount = 100 };
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync((User)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _expenseService.AddExpense(dto, "testuser"));
            Assert.That(ex.Message, Is.EqualTo("Target user not found"));
        }

        [Test]
        public async Task UpdateExpensePassTest()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var dto = new ExpenseUpdateRequestDto
            {
                Id = expenseId,
                Title = "Updated Title",
                Category = "Updated Food",
                Notes = "Updated notes",
                Amount = 150.75m
            };
            var existingExpense = new Expense 
            { 
                Id = expenseId, 
                Title = "Original Expense", 
                Category = "Food", 
                Amount = 100m,
                Username = "testuser"
            };
            var updatedExpense = new Expense 
            { 
                Id = expenseId, 
                Title = "Updated Title",
                Category = "Updated Food", 
                Amount = 150.75m,
                Username = "testuser"
            };

            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(existingExpense);
            _expenseRepositoryMock.Setup(r => r.Update(expenseId, It.IsAny<Expense>())).ReturnsAsync(updatedExpense);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _expenseService.UpdateExpense(dto, "testuser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Updated Title"));
            Assert.That(result.Category, Is.EqualTo("Updated Food"));
            Assert.That(result.Amount, Is.EqualTo(150.75m));
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void UpdateExpenseFailTest_ExpenseNotFound()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var dto = new ExpenseUpdateRequestDto { Id = expenseId, Category = "Updated" };
            
            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync((Expense)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _expenseService.UpdateExpense(dto, "testuser"));
            Assert.That(ex.Message, Is.EqualTo("Expense not found"));
        }

        [Test]
        public void UpdateExpenseFailTest_UpdateFailed()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var dto = new ExpenseUpdateRequestDto { Id = expenseId, Category = "Updated" };
            var existingExpense = new Expense { Id = expenseId, Category = "Food", Username = "testuser" };

            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(existingExpense);
            _expenseRepositoryMock.Setup(r => r.Update(expenseId, It.IsAny<Expense>())).ReturnsAsync((Expense)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityUpdateException>(async () => 
                await _expenseService.UpdateExpense(dto, "testuser"));
            Assert.That(ex.Message, Is.EqualTo("Could not update expense"));
        }

        [Test]
        public async Task DeleteExpensePassTest()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var expense = new Expense 
            { 
                Id = expenseId, 
                Title = "Test Expense", 
                Username = "testuser"
            };

            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(expense);
            _expenseRepositoryMock.Setup(r => r.Delete(expenseId)).ReturnsAsync(expense);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _expenseService.DeleteExpense(expenseId, "currentuser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expenseId));
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void DeleteExpenseFailTest_ExpenseNotFound()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync((Expense)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _expenseService.DeleteExpense(expenseId, "currentuser"));
            Assert.That(ex.Message, Is.EqualTo("Expense not found"));
        }

        [Test]
        public async Task GetExpensePassTest()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var expense = new Expense 
            { 
                Id = expenseId, 
                Title = "Test Expense",
                Amount = 100.50m,
                Category = "Food"
            };
            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(expense);

            // Act
            var result = await _expenseService.GetExpense(expenseId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expenseId));
            Assert.That(result.Title, Is.EqualTo("Test Expense"));
        }

        [Test]
        public void GetExpenseFailTest_ExpenseNotFound()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync((Expense)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _expenseService.GetExpense(expenseId));
            Assert.That(ex.Message, Is.EqualTo("Expense not found"));
        }

        [Test]
        public async Task SearchExpensePassTest()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new Expense { Title = "Food Expense", Category = "Food", Amount = 50, CreatedAt = DateTimeOffset.Now.AddDays(-1) },
                new Expense { Title = "Travel Expense", Category = "Travel", Amount = 200, CreatedAt = DateTimeOffset.Now }
            };
            var searchModel = new ExpenseSearchModel { Category = "Food" };
            
            _expenseRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(expenses);

            // Act
            var result = await _expenseService.SearchExpense(searchModel);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Category, Is.EqualTo("Food"));
            Assert.That(result.First().Title, Is.EqualTo("Food Expense"));
        }

        [Test]
        public async Task SearchExpenseFailTest_NoMatches()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new Expense { Title = "Food Expense", Category = "Food", Amount = 50, CreatedAt = DateTimeOffset.Now },
                new Expense { Title = "Travel Expense", Category = "Travel", Amount = 200, CreatedAt = DateTimeOffset.Now }
            };
            var searchModel = new ExpenseSearchModel { Category = "Entertainment" };
            
            _expenseRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(expenses);

            // Act
            var result = await _expenseService.SearchExpense(searchModel);

            // Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [TearDown]
        public void TearDown()
        {
            _expenseRepositoryMock = null;
            _userRepositoryMock = null;
            _receiptServiceMock = null;
            _auditLogServiceMock = null;
            _expenseService = null;
        }
    }
}