
using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Text;

 [TestFixture]
    public class ExpenseServiceTest
    {
        private Mock<IRepository<Guid, Expense>> _expenseRepositoryMock;
        private Mock<IRepository<string, User>> _userRepositoryMock;
        private Mock<IReceiptService> _receiptServiceMock;
        private Mock<IAuditLogService> _auditLogServiceMock;
        private ExpenseService _expenseService;

        [SetUp]
        public void Setup()
        {
            _expenseRepositoryMock = new Mock<IRepository<Guid, Expense>>();
            _userRepositoryMock = new Mock<IRepository<string, User>>();
            _receiptServiceMock = new Mock<IReceiptService>();
            _auditLogServiceMock = new Mock<IAuditLogService>();
            
            _expenseService = new ExpenseService(
                _expenseRepositoryMock.Object,
                _userRepositoryMock.Object,
                _receiptServiceMock.Object,
                _auditLogServiceMock.Object);
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
            // Remove this line if you don't want to test audit logging:
            // _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _expenseService.AddExpense(dto, "testuser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Test Expense"));
            Assert.That(result.Amount, Is.EqualTo(100.50m));
            
            // Remove this verification if you don't want to test audit logging:
            // _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void AddExpenseFailTest_NullDto()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _expenseService.AddExpense(null, "testuser"));
        }

        [Test]
        public void AddExpenseFailTest_UserNotFound()
        {
            // Arrange
            var dto = new ExpenseAddRequestDto { Title = "Test", Amount = 100 };
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync((User)null);

            // Act & Assert
            Assert.ThrowsAsync<EntityNotFoundException>(async () => await _expenseService.AddExpense(dto, "testuser"));
        }

        [Test]
        public async Task GetExpensePassTest()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var expense = new Expense { Id = expenseId, Title = "Test Expense" };
            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(expense);

            // Act
            var result = await _expenseService.GetExpense(expenseId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expenseId));
        }

        [Test]
        public void GetExpenseFailTest_InvalidId()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync((Expense)null);

            // Act & Assert
            Assert.ThrowsAsync<EntityNotFoundException>(async () => await _expenseService.GetExpense(expenseId));
        }

        [Test]
        public async Task UpdateExpensePassTest()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var dto = new ExpenseUpdateRequestDto
            {
                Id = expenseId,
                Category = "Updated Category",
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
                Title = "Original Expense", 
                Category = "Updated Category", 
                Amount = 150.75m,
                Username = "testuser"
            };

            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(existingExpense);
            _expenseRepositoryMock.Setup(r => r.Update(expenseId, It.IsAny<Expense>())).ReturnsAsync(updatedExpense);

            // Act
            var result = await _expenseService.UpdateExpense(dto, "testuser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Category, Is.EqualTo("Updated Category"));
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
            Assert.ThrowsAsync<EntityNotFoundException>(async () => await _expenseService.UpdateExpense(dto, "testuser"));
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
                Username = "testuser",
                Receipt = null,
            };
            var user = new User 
            { 
                Username = "testuser", 
                Expenses = new List<Expense> { expense } 
            };

            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(expense);
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.Update("testuser", It.IsAny<User>())).ReturnsAsync(user);
            _expenseRepositoryMock.Setup(r => r.Delete(expenseId)).ReturnsAsync(expense);

            // Act
            var result = await _expenseService.DeleteExpense(expenseId,"currentuser");

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
            Assert.ThrowsAsync<EntityNotFoundException>(async () => await _expenseService.DeleteExpense(expenseId,"currentuser"));
        }



        [Test]
        public async Task SearchExpensePassTest()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new Expense { Title = "Food Expense", Category = "Food", Amount = 50, CreatedAt = DateTimeOffset.Now },
                new Expense { Title = "Travel Expense", Category = "Travel", Amount = 200, CreatedAt = DateTimeOffset.Now }
            };
            var searchModel = new ExpenseSearchModel { Category = "Food" };
            
            _expenseRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(expenses);

            // Act
            var result = await _expenseService.SearchExpense(searchModel);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Category, Is.EqualTo("Food"));
        }
    }
