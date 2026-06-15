using System.ComponentModel.DataAnnotations;

namespace DemoApp.Web.Models
{

        public class AuditLog
        {
            public int AuditLogId { get; set; }

            [MaxLength(100)]
            public string EntityName { get; set; } = string.Empty;

            public string? EntityId { get; set; }

            [MaxLength(100)]
            public string PerformedBy { get; set; } = string.Empty;

            public DateTime PerformedAt { get; set; }

            [MaxLength(20)]
            public string OperationType { get; set; } = string.Empty; // Added, Modified, Deleted

            public string? OldValues { get; set; } // JSON
            public string? NewValues { get; set; } // JSON

 
    }


}
