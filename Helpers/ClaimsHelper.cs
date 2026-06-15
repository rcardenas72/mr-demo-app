using System.Security.Claims;


namespace DemoApp.Web.Helpers
{
    public static class ClaimsHelper
    {
        public static string GetUserName(ClaimsPrincipal user)
        {
            var fullUsername = user?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? string.Empty;
            var atIndex = fullUsername.IndexOf('@');

            var username = atIndex >= 0
                ? fullUsername.Substring(0, atIndex)
                : fullUsername;

            return username;

        }

        public static string GetFullName(ClaimsPrincipal user)
        {
            return user?.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty;
        }

        public static string GetEmail(ClaimsPrincipal user)
        {
            return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value ?? string.Empty;
        }

        public static string GetUserId(ClaimsPrincipal user)
        {
            return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "oid")?.Value ?? string.Empty;
        }

        public static string GetRole(ClaimsPrincipal user)
        {
            return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "roles")?.Value ?? string.Empty;
        }

        public static Dictionary<string, string> GetAllClaims(ClaimsPrincipal user)
        {
            return user?.Claims.ToDictionary(c => c.Type, c => c.Value) ?? new Dictionary<string, string>();
        }
    }
}
