using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Auth;
using TMS_SharedLibrary.Utilities;
using TMS_SharedLibrary.Models;
using TMS_SharedLibrary.Audit.Enums;

namespace TMS_API.Middleware;

public class AuditLoginMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, bool> _loggedUsers = new();

    public AuditLoginMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userOid = context.User.GetOid();

        // LOGIN LOGGING
        if (context.User.Identity?.IsAuthenticated == true &&
            userOid != null &&
            !_loggedUsers.ContainsKey(userOid))
        {
            var auditLogger = context.RequestServices.GetRequiredService<IAuditLogger>();

            AuditHelper.LogAudit(
                auditLogger,
                context,
                AuditAction.Login,
                nameof(User),
                0, // replace with actual UserId if available
                "Logged in"
            );

            // Track that this user is logged in
            _loggedUsers[userOid] = true;
        }

        // LOGOUT LOGGING
        // Detect logout request (Microsoft Identity default path)
        if (context.Request.Path.StartsWithSegments("/MicrosoftIdentity/Account/SignOut", StringComparison.OrdinalIgnoreCase))
        {
            var auditLogger = context.RequestServices.GetRequiredService<IAuditLogger>();

            if (userOid != null && _loggedUsers.ContainsKey(userOid))
            {
                AuditHelper.LogAudit(
                    auditLogger,
                    context,
                    AuditAction.Logout,
                    nameof(User),
                    0,
                    "Logged out"
                );

                _loggedUsers.TryRemove(userOid, out _);
            }
        }

        // Continue pipeline
        await _next(context);
    }
}