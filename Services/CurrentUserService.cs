using DemoApp.Web.Helpers;
using DemoApp.Web.Services.Interfaces;

namespace DemoApp.Web.Services
{
    public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null ? ClaimsHelper.GetUserName(user) : "Unknown";
    }
}
}