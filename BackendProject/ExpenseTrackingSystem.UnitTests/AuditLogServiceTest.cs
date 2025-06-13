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

namespace ExpenseTrackingSystem.Tests
{
    [TestFixture]
    public class AuditLogServiceTest
    {
        private Mock<IRepository<Guid, AuditLog>> _auditRepositoryMock;
        private AuditLogService _auditLogService;

        [SetUp]
        public void Setup()
        {
            _auditRepositoryMock = new Mock<IRepository<Guid, AuditLog>>();
            _auditLogService = new AuditLogService(_auditRepositoryMock.Object);
        }

        [Test]
        public async Task GetAuditLogsTestPass()
        {
            // Arrange
            var audits = new List<AuditLog>
            {
                new AuditLog { Id = Guid.NewGuid(), Action = "Create", EntityName = "User" },
                new AuditLog { Id = Guid.NewGuid(), Action = "Update", EntityName = "Expense" }
            };
            _auditRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(audits);

            // Act
            var result = await _auditLogService.GetAllAuditLogs();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result, Is.InstanceOf<List<AuditLog>>());
        }

        [Test]
        public void GetAuditLogsTestFail()
        {
            // Arrange
            _auditRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(new List<AuditLog>());

            // Act & Assert
            Assert.ThrowsAsync<CollectionEmptyException>(async () => await _auditLogService.GetAllAuditLogs());
        }

        [Test]
        public async Task LogActionTestPass()
        {
            // Arrange
            var dto = new AuditAddRequestDto
            {
                Username = "testuser",
                Action = "Create",
                EntityName = "User",
                Details = "User created"
            };
            var audit = new AuditLog { Id = Guid.NewGuid(), Action = "Create", EntityName = "User" };
            _auditRepositoryMock.Setup(r => r.Add(It.IsAny<AuditLog>())).ReturnsAsync(audit);

            // Act
            var result = await _auditLogService.LogAction(dto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Action, Is.EqualTo("Create"));
        }

        [Test]
        public void LogActionTestFail()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _auditLogService.LogAction(null));
        }
    }
}