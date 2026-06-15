using Microsoft.Extensions.Diagnostics.HealthChecks;
using DemoApp.Web.Data;

namespace DemoApp.Web.Middleware
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(AppDbContext db, ILogger<DatabaseHealthCheck> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                await _db.Database.CanConnectAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed: cannot connect to database");
                return HealthCheckResult.Unhealthy("Database unavailable");
            }
        }
    }
}
