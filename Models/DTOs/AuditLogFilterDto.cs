namespace DemoApp.Web.Models.DTOs
{
    public class AuditLogFilterDto
    {
        public DateTime? PerformedFrom { get; set; }
        public DateTime? PerformedTo { get; set; }
        public string? PerformedBy { get; set; }
        public string? EntityName { get; set; }
        public string? OperationType { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}
