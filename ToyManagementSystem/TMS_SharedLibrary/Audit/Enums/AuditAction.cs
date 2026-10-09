using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_SharedLibrary.Audit.Enums
{
    public enum AuditAction
    {
        Create,
        Update,
        Delete,
        Borrow,
        Return,
        Login,
        Logout,
        System
    }
}