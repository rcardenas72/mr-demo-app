using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DemoApp.Web.Data;
using DemoApp.Web.Models;

namespace DemoApp.Web.Services
{
    public class AuditLogCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditLogCleanupService> _logger;
        private readonly AuditLogSettings _settings;

        public AuditLogCleanupService(
            IServiceProvider serviceProvider,
            ILogger<AuditLogCleanupService> logger,
            IOptions<AuditLogSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddHours(2);
                var delay = nextRun - now;

                _logger.LogInformation("Audit cleanup scheduled for {NextRun}", nextRun);
                await Task.Delay(delay, stoppingToken);

                try
                {
                    await PurgeOldLogsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during audit log cleanup");
                }
            }
        }

        private async Task PurgeOldLogsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-_settings.RetentionDays);
            var deleted = await context.AuditLogs
                .Where(a => a.PerformedAt < cutoff)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Audit cleanup: purged {Count} logs older than {CutoffDate}", deleted, cutoff);
        }
    }
}
