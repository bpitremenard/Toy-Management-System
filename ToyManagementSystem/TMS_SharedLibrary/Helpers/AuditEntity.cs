using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_SharedLibrary.Helpers
{
    public static class AuditEntity
    {
        public static string From<T>()
            => typeof(T).Name;

        public static string From(string entityName) => entityName;
    }
}
