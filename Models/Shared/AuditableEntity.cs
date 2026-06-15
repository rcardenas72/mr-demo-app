using DemoApp.Web.Models.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Web.Models.Shared
{
    public abstract class AuditableEntity
    {
        [NotAudited]
        [MaxLength(100)]
        public string InsUser { get; set; } = string.Empty;

        [NotAudited]
        public DateTime InsDate { get; set; } = DateTime.UtcNow;

        [NotAudited]
        [MaxLength(100)]
        public string? UpdUser { get; set; }

        [NotAudited]
        public DateTime? UpdDate { get; set; }
    }

}
