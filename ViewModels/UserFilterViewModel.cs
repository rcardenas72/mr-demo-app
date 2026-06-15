using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemoApp.Web.ViewModels
{
    public class UserFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 7;
        public int TotalPages { get; set; }

        public List<SelectListItem>? Roles { get; set; }
        public List<UserViewModel> Users { get; set; } = new();
    }
}
