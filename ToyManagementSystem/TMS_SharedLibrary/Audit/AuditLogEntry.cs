using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS_SharedLibrary.Audit.Enums;

namespace TMS_SharedLibrary.Audit
{
    public class AuditLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public AuditAction Action { get; set; }

        public string Entity { get; set; } = default!;

        public string EntityId { get; set; } = default!;

        public string? PerformedByOid { get; set; }

        public string Role { get; set; } = "Unknown";

        public string? Details { get; set; }

        public string Level { get; set; } = "INFO";
    }
}
