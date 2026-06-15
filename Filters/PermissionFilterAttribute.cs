using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoApp.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PermissionFilterAttribute : ActionFilterAttribute
    {
        private readonly OperationType _operationType;

        public PermissionFilterAttribute(OperationType operationType)
        {
            _operationType = operationType;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var permissionRepository = httpContext.RequestServices.GetRequiredService<IPermissionRepository>();
            var claimsPrincipal = httpContext.User;

            var email = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                        ?? claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var username = !string.IsNullOrWhiteSpace(email) ? email.Split('@')[0] : null;

            if (string.IsNullOrEmpty(username))
                username = httpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }

            var isAdmin = httpContext.Session.GetString("IsAdmin") == "true";
            var controllerName = context.RouteData.Values["controller"]?.ToString();

            if (string.IsNullOrEmpty(controllerName))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }

            if (isAdmin)
            {
                httpContext.Items["CanAdd"] = true;
                httpContext.Items["CanEdit"] = true;
                httpContext.Items["CanDelete"] = true;
                await next();
                return;
            }

            var cache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var cacheKey = $"permissions_{username}";
            var permissions = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await permissionRepository.GetPermissions(username);
            });
            var controllerPermissions = permissions!.Where(p => p.Menu.ControllerName == controllerName).ToList();

            foreach (var p in controllerPermissions)
            {
                switch (p.OperationType)
                {
                    case OperationType.Agregar:
                        httpContext.Items["CanAdd"] = true;
                        break;
                    case OperationType.Editar:
                        httpContext.Items["CanEdit"] = true;
                        break;
                    case OperationType.Eliminar:
                        httpContext.Items["CanDelete"] = true;
                        break;
                }
            }

            var hasSpecificPermission = controllerPermissions.Any(p => p.OperationType == _operationType);
            if (!hasSpecificPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }

            await next();
        }

    }
}