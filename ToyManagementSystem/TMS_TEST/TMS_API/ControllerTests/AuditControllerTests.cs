using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TMS_API.Controllers;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Audit.Enums;
using Xunit;

namespace TMS_TEST.TMS_API.ControllerTests
{
    public class AuditControllerTests
    {
        [Fact]
        public async Task LogAudit_WithNullRequest_ReturnsBadRequest()
        {
            var mockLogger = new Mock<IAuditLogger>();
            var controller = new AuditController(mockLogger.Object);

            var result = await controller.LogAudit(null);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task LogAudit_WithValidRequest_CallsHelperAndReturnsOk()
        {
            var mockLogger = new Mock<IAuditLogger>();
            var controller = new AuditController(mockLogger.Object);

            var request = new AuditLogRequest
            {
                Action = AuditAction.Update,
                Entity = "Toy",
                EntityId = 123,
                Details = "updated toy details"
            };

            var result = await controller.LogAudit(request);

            Assert.IsType<OkResult>(result);
            mockLogger.Verify(l => l.Log(It.IsAny<AuditLogEntry>()), Times.Once);
        }

        [Fact]
        public async Task LogAudit_WithAuthenticatedContext_UsesUserInfo()
        {
            var mockLogger = new Mock<IAuditLogger>();

            var controller = new AuditController(mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("oid", "user-789"),
                            new Claim(ClaimTypes.Name, "ControllerUser"),
                            new Claim(ClaimTypes.Role, "Teacher")
                        }, "mock"))
                    }
                }
            };

            var request = new AuditLogRequest
            {
                Action = AuditAction.System,
                Entity = "Toy",
                EntityId = 7,
                Details = "read toy info"
            };

            var result = await controller.LogAudit(request);

            Assert.IsType<OkResult>(result);
            mockLogger.Verify(l => l.Log(It.Is<AuditLogEntry>(e =>
                e.PerformedByOid == "user-789" &&
                e.Role == "Teacher" &&
                e.Details.Contains("ControllerUser")
            )), Times.Once);
        }
    }
}