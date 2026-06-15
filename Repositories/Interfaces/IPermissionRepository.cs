using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;

namespace DemoApp.Web.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetPermissions(string username);
        Task<Permission?> GetByIdAsync(int id);
        Task<List<string>> GetAccessibleMenuOptionsByUsernameAsync(string username);
        Task<List<Permission>> GetByRoleIdAsync(int roleId);
        Task CreateAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task DeleteAsync(int id);
        Task<List<Permission>> GetPagedByRoleIdAsync(int roleId, int page, int pageSize);
        Task<int> CountByRoleIdAsync(int roleId);
        Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType);
        Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType, int excludeId);
    }
}
