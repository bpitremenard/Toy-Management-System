using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TMS_SharedLibrary.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetOid(this ClaimsPrincipal user)
        {

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Common OID claim types
            string? oid = user.FindFirst("oid")?.Value
                         ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                         ?? user.FindFirst("sub")?.Value;

            return oid;
        }



        public static string GetRole(this ClaimsPrincipal user)
        {
            var role = user.Claims.FirstOrDefault(c =>
                c.Type == "roles" ||
                c.Type == ClaimTypes.Role ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            )?.Value;

            return role ?? "Unknown";
        }
    }


}
