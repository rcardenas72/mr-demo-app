using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;

namespace DemoApp.Web.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(int id);
        Task<List<AuditLog>> GetByEntityAsync(string entityName);
        Task<(List<AuditLog> Logs, int TotalCount)> GetFilteredAsync(AuditLogFilterDto filter);
        Task<int> PurgeAllAsync();
    }
}
