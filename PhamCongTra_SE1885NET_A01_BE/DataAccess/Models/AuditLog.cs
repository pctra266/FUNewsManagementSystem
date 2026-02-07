using System;

namespace DataAccess.Models
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public short UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public virtual SystemAccount User { get; set; } = null!;
    }
}
