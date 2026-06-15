using DemoApp.Web.Models;
using DemoApp.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DemoApp.Web.Models.Interfaces;
using DemoApp.Web.Helpers;
using DemoApp.Web.Models.Shared;

namespace DemoApp.Web.Data
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return result;

            var auditLogs = new List<AuditLog>();
            var currentUser = _currentUserService.GetUsername();

            foreach (var entry in context.ChangeTracker.Entries().Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted))
            {
                if (entry.Entity is AuditLog) continue;
                if (entry.Entity is INotAuditableEntity) continue;

                // Auditoría de campos comunes
                if (entry.Entity is AuditableEntity auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.InsUser = currentUser;
                        auditable.InsDate = DateTime.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditable.UpdUser = currentUser;
                        auditable.UpdDate = DateTime.UtcNow;
                    }
                }

                var auditLog = new AuditLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    OperationType = entry.State.ToString(),
                    PerformedAt = DateTime.UtcNow,
                    PerformedBy = currentUser
                };

                // Captura de claves
                if (entry.State != EntityState.Added)
                {
                    var pk = entry.Metadata.FindPrimaryKey()?.Properties;
                    if (pk != null)
                    {
                        var keyValues = pk.ToDictionary(
                            prop => prop.Name,
                            prop => entry.Property(prop.Name).CurrentValue!
                        );
                        auditLog.EntityId = JsonSerializer.Serialize(keyValues);
                    }
                }

                // Cache de propiedades ignoradas
                var ignoredProperties = ReflectionCacheHelper.GetNotAuditedProperties(entry.Entity.GetType());

                // OldValues
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                {
                    var oldValues = entry.Properties
                        .Where(p => (p.IsModified || entry.State == EntityState.Deleted) &&
                                    !ignoredProperties.Contains(p.Metadata.Name))
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

                    auditLog.OldValues = JsonSerializer.Serialize(oldValues);
                }

                // NewValues
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    var newValues = entry.Properties
                        .Where(p => (p.IsModified || entry.State == EntityState.Added) &&
                                    !ignoredProperties.Contains(p.Metadata.Name))
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                    auditLog.NewValues = JsonSerializer.Serialize(newValues);
                }

                // Evita registrar si no hay cambios relevantes
                if (string.IsNullOrEmpty(auditLog.OldValues) && string.IsNullOrEmpty(auditLog.NewValues))
                    continue;

                auditLogs.Add(auditLog);
            }

            if (auditLogs.Any())
                await context.Set<AuditLog>().AddRangeAsync(auditLogs, cancellationToken);

            return result;
        }
    }
}
