using Microsoft.AspNetCore.Mvc;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Utilities;

namespace TMS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditLogger _auditLogger;

    public AuditController(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    [HttpPost("log")]
    public Task<IActionResult> LogAudit([FromBody] AuditLogRequest request)
    {
        if (request == null) return Task.FromResult<IActionResult>(BadRequest());

        AuditHelper.LogAudit(
            _auditLogger,
            HttpContext,
            request.Action,
            request.Entity,
            request.EntityId,
            request.Details
        );

        return Task.FromResult<IActionResult>(Ok());
    }
}