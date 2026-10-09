using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS_SharedLibrary.Audit.Enums;

namespace TMS_SharedLibrary.Audit
{
    // DTO sent from Student/Teacher projects
    public class AuditLogRequest //create a new DTO for the audit log request, which will be sent from the Student/Teacher projects. This will allow us to decouple the API from the internal AuditLogEntry model and only expose the necessary fields.
    {
        public AuditAction Action { get; set; }
        public string Entity { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}
