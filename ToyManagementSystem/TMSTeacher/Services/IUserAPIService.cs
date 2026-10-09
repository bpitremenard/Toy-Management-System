using TMS_SharedLibrary.Models;
using TMS_SharedLibrary.Audit.Enums;

namespace TMSTeacher.Services
{
    public interface IUserAPIService
    {
        Task<User> GetUserByOidAsync(string oid);
        Task<User> CreateUserAsync(User user);
        Task LogAuditAsync(AuditAction action, string entity, int entityId, string details);
    }
}
