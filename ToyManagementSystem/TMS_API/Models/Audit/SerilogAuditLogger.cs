using Serilog;
using TMS_SharedLibrary.Audit;

namespace TMS_API.Audit
{
    public class SerilogAuditLogger : IAuditLogger
    {
        private readonly Serilog.ILogger _logger;

        public SerilogAuditLogger()
        {
            _logger = Serilog.Log.ForContext<SerilogAuditLogger>();
        }

        public void Log(AuditLogEntry entry)
        {
            switch (entry.Level.ToUpper())
            {
                case "WARNING":
                    _logger.Warning(FormatEntry(entry));
                    break;
                case "ERROR":
                    _logger.Error(FormatEntry(entry));
                    break;
                default:
                    _logger.Information(FormatEntry(entry));
                    break;
            }
        }

        private string FormatEntry(AuditLogEntry entry)
        {
            return $"[{entry.Level}] {entry.Timestamp:yyyy-MM-dd HH:mm:ss} | Action: {entry.Action} | Entity: {entry.Entity} | EntityId: {entry.EntityId} | PerformedBy: {entry.PerformedByOid} | Role: {entry.Role} | Details: {entry.Details}";
        }
    }

}
