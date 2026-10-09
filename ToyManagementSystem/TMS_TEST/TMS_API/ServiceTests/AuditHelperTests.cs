using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Audit.Enums;
using TMS_SharedLibrary.Utilities;
using Xunit;

namespace TMS_TEST.TMS_API.ServiceTests
{
    public class AuditHelperTests
    {
        [Fact]
        public void LogAudit_WithAuthenticatedUser_LogsCorrectEntry()
        {
            // Arrange
            var mockLogger = new Mock<IAuditLogger>();
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("oid", "user-123"),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            // Act
            AuditHelper.LogAudit(
                mockLogger.Object,
                context,
                AuditAction.Create,
                "Toy",
                42,
                "created a new toy"
            );

            // Assert
            mockLogger.Verify(l => l.Log(It.Is<AuditLogEntry>(e =>
                e.Action == AuditAction.Create &&
                e.Entity == "Toy" &&
                e.EntityId == "42" &&
                e.PerformedByOid == "user-123" &&
                e.Role == "Admin" &&
                e.Details.Contains("TestUser")
            )), Times.Once);
        }

        [Fact]
        public void LogAudit_WithoutAuthenticatedUser_UsesDefaults()
        {
            // Arrange
            var mockLogger = new Mock<IAuditLogger>();
            HttpContext? context = null;

            // Act
            AuditHelper.LogAudit(
                mockLogger.Object,
                context,
                AuditAction.Delete,
                "Toy",
                99,
                "deleted toy"
            );

            // Assert
            mockLogger.Verify(l => l.Log(It.Is<AuditLogEntry>(e =>
                e.Action == AuditAction.Delete &&
                e.Entity == "Toy" &&
                e.EntityId == "99" &&
                e.PerformedByOid == "system" &&
                e.Role == "Unknown" &&
                e.Details.Contains("Unknown")
            )), Times.Once);
        }
    }
}
