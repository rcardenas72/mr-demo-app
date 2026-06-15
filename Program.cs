using DemoApp.Web.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using DemoApp.Web.Services;
using DemoApp.Web.Data;
using Microsoft.EntityFrameworkCore;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Prometheus;
using Serilog;
using DemoApp.Web.Mappings;
using DemoApp.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables("DemoApp__")
    .AddUserSecrets<Program>(optional: true);

// Configurar autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Error/AccessDenied";
    });

// Agregar soporte para sesi�n y acceso a HttpContext
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor(); // necesario para @inject IHttpContextAccessor

var mvcBuilder = builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
    // Pol�tica expl�cita que permite acceso an�nimo
    options.AddPolicy("AllowAnonymous", policy =>
    {
        policy.RequireAssertion(_ => true);
    });
});


builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("FormPolicy", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AuditInterceptor>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<AppMapper>();

builder.Services.Configure<AuditLogSettings>(
    builder.Configuration.GetSection("AuditLogSettings"));
builder.Services.AddHostedService<AuditLogCleanupService>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("Database");

//builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("DataBase"));

builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    var interceptor = provider.GetRequiredService<AuditInterceptor>();

    options.UseInMemoryDatabase("DataBase");

    options.AddInterceptors(interceptor);
});

builder.Services.AddMetrics(); // para exponer metricas a prometeus
var app = builder.Build();

// EnsureCreated + seed data se ejecuta siempre (no solo en Development)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();

//Seguridad de cabeceras con CSP nonce
app.Use(async (context, next) =>
{
    var nonce = Guid.NewGuid().ToString("N");
    context.Items["ScriptNonce"] = nonce;
    context.Items["StyleNonce"] = nonce;

    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        $"default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self'; img-src 'self' data:; font-src 'self'; frame-ancestors 'none';";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
    await next();
});

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}

app.UseRouting();

app.UseRateLimiter();

app.UseMiddleware<MetricsAuthMiddleware>();

app.UseHttpMetrics();
app.UseMetricServer();

app.UseAuthentication();

app.UseSession();
app.UseMiddleware<EnsureSessionMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapMetrics().RequireAuthorization("AllowAnonymous");
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, result) =>
    {
        ctx.Response.ContentType = "text/plain";
        await ctx.Response.WriteAsync(result.Status.ToString());
    }
});

await app.RunAsync();
