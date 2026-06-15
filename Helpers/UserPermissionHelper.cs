using System.Text.Json;

namespace DemoApp.Web.Helpers
{
    public static class UserPermissionHelper
    {
        public static List<string> GetPermissions(HttpContext context)
        {
            var permissionsJson = context.Session.GetString("UserPermissions");

            return string.IsNullOrEmpty(permissionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(permissionsJson) ?? new List<string>();
        }

        public static bool IsAdmin(HttpContext context)
        {
            return context.Session.GetString("IsAdmin") == "true";
        }
    }
}
