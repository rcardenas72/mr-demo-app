namespace DemoApp.Web.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
    }
}
