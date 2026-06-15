namespace DemoApp.Web.Middleware
{
    public class EnsureSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public EnsureSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si el usuario no está autenticado, continúa normal
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Evitar redirección en loop o rutas abiertas
    var isSafePath =
        path.StartsWith("/account/login") ||
        path.StartsWith("/account/logout") ||
        path.StartsWith("/error") ||
        path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/images");

    if (isSafePath)
    {
        await _next(context);
        return;
    }

    // Si no se ha cargado la sesión aún, redirige al login
    if (string.IsNullOrEmpty(context.Session.GetString("UserName")))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

            await _next(context);
        }
    }
}
