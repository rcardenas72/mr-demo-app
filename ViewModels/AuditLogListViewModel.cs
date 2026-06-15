using DemoApp.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemoApp.Web.ViewModels
{

    public class AuditLogListViewModel
    {
        public List<AuditLog> Logs { get; set; } = new();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        // Filtros
        public string? PerformedBy { get; set; }
        public string? EntityName { get; set; }
        public string? OperationType { get; set; }
        public DateTime? PerformedFrom { get; set; }
        public DateTime? PerformedTo { get; set; }

        public PagedResultViewModel<AuditLog> PagedResult { get; set; } = new();

        // Listas para dropdowns
        public List<SelectListItem> EntityOptions { get; set; } = new();
        public List<SelectListItem> OperationTypeOptions { get; set; } = new();


    }

}
