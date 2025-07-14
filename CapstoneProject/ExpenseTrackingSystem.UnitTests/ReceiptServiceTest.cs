using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Text;

namespace ExpenseTrackingSystem.Tests
{
    [TestFixture]
    public class ReceiptServiceTest
    {
        private Mock<IRepository<Guid, Receipt>> _receiptRepositoryMock;
        private Mock<IRepository<Guid, Expense>> _expenseRepositoryMock;
        private Mock<IRepository<string, User>> _userRepositoryMock;
        private Mock<IAuditLogService> _auditLogServiceMock;
        private ReceiptService _receiptService;
        private Mock<ILogger<ReceiptService>> _mockLogger;
        private string _testStoragePath = Path.Combine(Path.GetTempPath(), "test_receipts");

        [SetUp]
        public void Setup()
        {
            _receiptRepositoryMock = new Mock<IRepository<Guid, Receipt>>();
            _expenseRepositoryMock = new Mock<IRepository<Guid, Expense>>();
            _userRepositoryMock = new Mock<IRepository<string, User>>();
            _auditLogServiceMock = new Mock<IAuditLogService>();
            _mockLogger = new Mock<ILogger<ReceiptService>>();
            
            _receiptService = new ReceiptService(
                _receiptRepositoryMock.Object,
                _expenseRepositoryMock.Object,
                _userRepositoryMock.Object,
                _auditLogServiceMock.Object,
                _mockLogger.Object,
                _testStoragePath);
        }

        [Test]
        public async Task CreateReceiptPassTest()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var receiptId = Guid.NewGuid();
            var mockFile = new Mock<IFormFile>();
            var content = "Test file content";
            var fileName = "test_receipt.jpg";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            var dto = new ReceiptAddRequestDto
            {
                ReceiptBill = mockFile.Object,
                ReceiptName = "test_receipt",
                Username = "testuser",
                Category = "Food",
                ExpenseId = expenseId
            };

            var expense = new Expense { Id = expenseId, Username = "testuser" };
            var user = new User { Username = "testuser", Receipts = new List<Receipt>() };
            var receipt = new Receipt 
            { 
                Id = receiptId, 
                ReceiptName = "test_receipt",
                Username = "testuser",
                ExpenseId = expenseId
            };

            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync(expense);
            _userRepositoryMock.Setup(r => r.GetByID("testuser")).ReturnsAsync(user);
            _receiptRepositoryMock.Setup(r => r.Add(It.IsAny<Receipt>())).ReturnsAsync(receipt);
            // ✅ REMOVED: No longer need expense update setup since ReceiptId doesn't exist
            _userRepositoryMock.Setup(r => r.Update("testuser", It.IsAny<User>())).ReturnsAsync(user);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _receiptService.CreateReceipt(dto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReceiptName, Is.EqualTo("test_receipt"));
            Assert.That(result.Username, Is.EqualTo("testuser"));
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void CreateReceiptFailTest_NullDto()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _receiptService.CreateReceipt(null));
            Assert.That(ex.ParamName, Is.EqualTo("dto"));
            Assert.That(ex.Message, Does.Contain("Receipt cannot be null"));
        }

        [Test]
        public void CreateReceiptFailTest_ExpenseNotFound()
        {
            // Arrange
            var expenseId = Guid.NewGuid();
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            var dto = new ReceiptAddRequestDto
            {
                ReceiptBill = mockFile.Object,
                ReceiptName = "test_receipt",
                Username = "testuser",
                ExpenseId = expenseId
            };

            _expenseRepositoryMock.Setup(r => r.GetByID(expenseId)).ReturnsAsync((Expense)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _receiptService.CreateReceipt(dto));
            Assert.That(ex.Message, Is.EqualTo("Expense not found"));
        }

        [Test]
        public async Task DeleteReceiptPassTest()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            var receipt = new Receipt 
            { 
                Id = receiptId,
                ReceiptName = "test_receipt",
                Username = "testuser",
                FilePath = Path.Combine(_testStoragePath, "receipt.jpg")
            };

            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync(receipt);
            // ✅ REMOVED: No user or expense update setups since they're handled by cascade delete
            _receiptRepositoryMock.Setup(r => r.Delete(receiptId)).ReturnsAsync(receipt);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _receiptService.DeleteReceipt(receiptId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(receiptId));
            _receiptRepositoryMock.Verify(r => r.Delete(receiptId), Times.Once);
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void DeleteReceiptFailTest_ReceiptNotFound()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync((Receipt)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _receiptService.DeleteReceipt(receiptId));
            Assert.That(ex.Message, Is.EqualTo("Could not find receipt"));
        }

        [Test]
        public async Task GetReceiptPassTest()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();
            var testFilePath = Path.Combine(_testStoragePath, "test_receipt.jpg");
            var testContent = "Test file content";
            
            var receipt = new Receipt 
            { 
                Id = receiptId,
                ReceiptName = "test_receipt",
                Username = "testuser",
                ExpenseId = expenseId,
                Category = "Food",
                CreatedAt = DateTimeOffset.UtcNow,
                FilePath = testFilePath
            };

            // Create test directory and file
            Directory.CreateDirectory(_testStoragePath);
            await File.WriteAllTextAsync(testFilePath, testContent);

            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync(receipt);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            try
            {
                // Act
                var result = await _receiptService.GetReceipt(receiptId);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.ReceiptName, Is.EqualTo("test_receipt"));
                Assert.That(result.Username, Is.EqualTo("testuser"));
                Assert.That(result.fileData, Is.Not.Null);
                // ✅ VERIFY: Check that audit log is called with "Read" action
                _auditLogServiceMock.Verify(a => a.LogAction(It.Is<AuditAddRequestDto>(x => x.Action == "Read")), Times.Once);
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
            }
        }

        [Test]
        public void GetReceiptFailTest_ReceiptNotFound()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync((Receipt)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _receiptService.GetReceipt(receiptId));
            Assert.That(ex.Message, Is.EqualTo("Receipt not found"));
        }

        [Test]
        public void GetReceiptFailTest_FileNotFound()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            var receipt = new Receipt 
            { 
                Id = receiptId,
                FilePath = "/nonexistent/path/receipt.jpg"
            };

            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync(receipt);

            // Act & Assert
            var ex = Assert.ThrowsAsync<FileNotFoundException>(async () => 
                await _receiptService.GetReceipt(receiptId));
            Assert.That(ex.Message, Is.EqualTo("No receipt file found"));
        }

        [Test]
        public async Task UpdateReceiptPassTest_WithoutFileUpdate()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            var dto = new ReceiptUpdateRequestDto
            {
                ReceiptName = "updated_receipt",
                Category = "Updated Category"
            };
            var existingReceipt = new Receipt 
            { 
                Id = receiptId,
                ReceiptName = "original_receipt",
                Category = "Food",
                Username = "testuser"
            };
            var updatedReceipt = new Receipt 
            { 
                Id = receiptId,
                ReceiptName = "updated_receipt",
                Category = "Updated Category",
                Username = "testuser"
            };

            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync(existingReceipt);
            _receiptRepositoryMock.Setup(r => r.Update(receiptId, It.IsAny<Receipt>())).ReturnsAsync(updatedReceipt);
            _auditLogServiceMock.Setup(a => a.LogAction(It.IsAny<AuditAddRequestDto>())).ReturnsAsync(new AuditLog());

            // Act
            var result = await _receiptService.UpdateReceipt(dto, receiptId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReceiptName, Is.EqualTo("updated_receipt"));
            Assert.That(result.Category, Is.EqualTo("Updated Category"));
            _auditLogServiceMock.Verify(a => a.LogAction(It.IsAny<AuditAddRequestDto>()), Times.Once);
        }

        [Test]
        public void UpdateReceiptFailTest_ReceiptNotFound()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            var dto = new ReceiptUpdateRequestDto
            {
                ReceiptName = "updated_receipt"
            };

            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync((Receipt)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () => 
                await _receiptService.UpdateReceipt(dto, receiptId));
            Assert.That(ex.Message, Is.EqualTo("Receipt not found"));
        }

        [Test]
        public void UpdateReceiptFailTest_UpdateFailed()
        {
            // Arrange
            var receiptId = Guid.NewGuid();
            var dto = new ReceiptUpdateRequestDto
            {
                ReceiptName = "updated_receipt"
            };
            var existingReceipt = new Receipt 
            { 
                Id = receiptId,
                ReceiptName = "original_receipt",
                Username = "testuser"
            };

            _receiptRepositoryMock.Setup(r => r.GetByID(receiptId)).ReturnsAsync(existingReceipt);
            _receiptRepositoryMock.Setup(r => r.Update(receiptId, It.IsAny<Receipt>())).ReturnsAsync((Receipt)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityUpdateException>(async () => 
                await _receiptService.UpdateReceipt(dto, receiptId));
            Assert.That(ex.Message, Is.EqualTo("Could not update receipt"));
        }

        [Test]
        public async Task SearchReceiptsPassTest()
        {
            // Arrange
            var receipts = new List<Receipt>
            {
                new Receipt 
                { 
                    ReceiptName = "food_receipt", 
                    Category = "Food", 
                    CreatedAt = DateTimeOffset.Now.AddDays(-1) 
                },
                new Receipt 
                { 
                    ReceiptName = "travel_receipt", 
                    Category = "Travel", 
                    CreatedAt = DateTimeOffset.Now 
                }
            };
            var searchModel = new ReceiptSearchModel { Category = "Food" };
            
            _receiptRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(receipts);

            // Act
            var result = await _receiptService.SearchReceipts(searchModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Category, Is.EqualTo("Food"));
        }

        [Test]
        public async Task SearchReceiptsFailTest_NoMatches()
        {
            // Arrange
            var receipts = new List<Receipt>
            {
                new Receipt 
                { 
                    ReceiptName = "food_receipt", 
                    Category = "Food", 
                    CreatedAt = DateTimeOffset.Now 
                },
                new Receipt 
                { 
                    ReceiptName = "travel_receipt", 
                    Category = "Travel", 
                    CreatedAt = DateTimeOffset.Now 
                }
            };
            var searchModel = new ReceiptSearchModel { Category = "Entertainment" }; // No matches
            
            _receiptRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(receipts);

            // Act
            var result = await _receiptService.SearchReceipts(searchModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task SearchReceiptsFailTest_ExceptionHandling()
        {
            // Arrange
            var searchModel = new ReceiptSearchModel { Category = "Food" };
            
            _receiptRepositoryMock.Setup(r => r.GetAll()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _receiptService.SearchReceipts(searchModel);

            // Assert
            Assert.That(result, Is.Null); // Service returns null on exception
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                // Clean up any test files or directories
                if (Directory.Exists(_testStoragePath))
                {
                    Directory.Delete(_testStoragePath, true);
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors in tests
            }
            
            _receiptRepositoryMock = null;
            _expenseRepositoryMock = null;
            _userRepositoryMock = null;
            _auditLogServiceMock = null;
            _receiptService = null;
        }
    }
}