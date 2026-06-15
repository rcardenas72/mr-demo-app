using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;
using DemoApp.Web.Services.Utils;

namespace DemoApp.Web.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<Result<List<AuditLog>>> GetAllAsync();
        Task<Result<AuditLog?>> GetByIdAsync(int id);
        Task<Result<PagedResult<AuditLog>>> GetFilteredAsync(AuditLogFilterDto filter);
        Task<Result<int>> PurgeAllAsync();
    }
}
