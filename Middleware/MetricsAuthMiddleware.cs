namespace DemoApp.Web.Middleware
{
    public class MetricsAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;

        public MetricsAuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _apiKey = config["Metrics:ApiKey"] ?? "";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/metrics"))
            {
                if (string.IsNullOrEmpty(_apiKey) ||
                    !context.Request.Headers.TryGetValue("X-Api-Key", out var key) ||
                    key != _apiKey)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }

            await _next(context);
        }
    }
}
