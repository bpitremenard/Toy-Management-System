using Microsoft.AspNetCore.Http;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Audit.Enums;
using TMS_SharedLibrary.Auth;
using TMS_SharedLibrary.Helpers;
using TMS_SharedLibrary.Models;

namespace TMS_SharedLibrary.Utilities
{
    public static class AuditHelper
    {
        public static void LogAudit(
            IAuditLogger auditLogger,
            HttpContext? context,
            AuditAction action,
            string entity,
            int entityId,
            string details)
        {
            string performedByOid = "system";
            string performedByName = "Unknown";
            string role = "Unknown";

            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                performedByOid = context.User.GetOid() ?? "system";
                performedByName = context.User.Identity?.Name
                                  ?? context.User.FindFirst("name")?.Value
                                  ?? "Unknown";
                role = context.User.GetRole() ?? "Unknown";
            }

            auditLogger.Log(new AuditLogEntry
            {
                Action = action,
                Entity = AuditEntity.From(entity),
                EntityId = entityId.ToString(),
                PerformedByOid = performedByOid,
                Role = role,
                Details = $"{performedByName} {details}"
            });
        }
    }
}