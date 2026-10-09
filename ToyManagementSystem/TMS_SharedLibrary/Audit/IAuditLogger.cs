using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_SharedLibrary.Audit
{
    public interface IAuditLogger
    {
        void Log(AuditLogEntry entry);
    }
}
