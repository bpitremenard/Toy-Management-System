using TMS_SharedLibrary.Audit.Enums;
using TMS_SharedLibrary.Models;

namespace TMSStudent.Services
{
    public interface IUserAPIService
    {
        Task<User> GetUserByOidAsync(string oid);
        Task CreateUserAsync(User user);
        Task LogAuditAsync(AuditAction action, string entity, int entityId, string details);
    }
}
