using DemoApp.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemoApp.Web.ViewModels
{
    public class PermissionIndexViewModel
    {
        public int? SelectedRoleId { get; set; }
        public PagedResultViewModel<Permission> PagedPermissions { get; set; } = new();
        public List<SelectListItem> Roles { get; set; } = new();
    }
}
