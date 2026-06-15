using DemoApp.Web.Models;

namespace DemoApp.Web.ViewModels
{
    public class AuditLogDetailViewModel
    {
        public AuditLog Log { get; set; } = null!;

        // Parámetros para volver al listado con contexto
        public string? PerformedBy { get; set; }
        public string? EntityName { get; set; }
        public string? OperationType { get; set; }
        public DateTime? PerformedFrom { get; set; }
        public DateTime? PerformedTo { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
