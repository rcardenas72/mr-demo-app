namespace DemoApp.Web.Models.DTOs
{
    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 7;
    }
}
