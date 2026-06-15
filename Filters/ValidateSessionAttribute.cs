using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DemoApp.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var token = httpContext.Request.Cookies["AuthToken"];

            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
            if (!isAuthenticated || string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            httpContext.Items["AuthToken"] = token;
            base.OnActionExecuting(context);
        }
    }
}
