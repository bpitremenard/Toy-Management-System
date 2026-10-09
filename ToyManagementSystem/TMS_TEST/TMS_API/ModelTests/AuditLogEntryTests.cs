using System;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Audit.Enums;
using Xunit;

namespace TMS_TEST.TMS_API.ModelTests
{
    public class AuditLogEntryTests
    {
        [Fact]
        public void AuditLogEntry_DefaultsAreCorrect()
        {
            var entry = new AuditLogEntry
            {
                Action = AuditAction.Create,
                Entity = "Toy",
                EntityId = "1"
            };

            Assert.Equal("INFO", entry.Level); // default level
            Assert.Equal("Unknown", entry.Role); // default role
            Assert.NotNull(entry.Timestamp); // timestamp is set
            Assert.Equal("Toy", entry.Entity);
            Assert.Equal("1", entry.EntityId);
            Assert.Null(entry.PerformedByOid); // optional
            Assert.Null(entry.Details); // optional
        }

        [Fact]
        public void AuditLogEntry_CanSetAllProperties()
        {
            var timestamp = DateTime.UtcNow.AddMinutes(-5);
            var entry = new AuditLogEntry
            {
                Timestamp = timestamp,
                Action = AuditAction.Update,
                Entity = "Toy",
                EntityId = "42",
                PerformedByOid = "user-123",
                Role = "Admin",
                Details = "Updated details",
                Level = "WARNING"
            };

            Assert.Equal(timestamp, entry.Timestamp);
            Assert.Equal(AuditAction.Update, entry.Action);
            Assert.Equal("Toy", entry.Entity);
            Assert.Equal("42", entry.EntityId);
            Assert.Equal("user-123", entry.PerformedByOid);
            Assert.Equal("Admin", entry.Role);
            Assert.Equal("Updated details", entry.Details);
            Assert.Equal("WARNING", entry.Level);
        }
    }
}