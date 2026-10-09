using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Models;

namespace TMS_SharedLibrary.Helpers
{
    public class UserHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogger _auditLogger;

        public UserHelper(
            IHttpContextAccessor httpContextAccessor,
            IAuditLogger auditLogger)
        {
            _httpContextAccessor = httpContextAccessor;
            _auditLogger = auditLogger;
        }

        public Task<string> GetUserNameFromOidAsync(string? oid)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var name =
                user?.Identity?.Name ??
                user?.FindFirstValue("name") ??
                user?.FindFirstValue("preferred_username") ??
                "Unknown";

            return Task.FromResult(name);
        }
    }
}