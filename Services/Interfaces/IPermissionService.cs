using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Services.Utils;
using DemoApp.Web.ViewModels;

namespace DemoApp.Web.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<List<string>>> GetAccessibleMenuOptionsAsync(string userName);

        Task<Result<Permission?>> GetByIdAsync(int id);
        Task<Result> CreateAsync(PermissionFormViewModel model);
        Task<Result> UpdateAsync(PermissionFormViewModel model);
        Task<Result> DeleteAsync(int id);

        Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType);
        Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType, int excludeId);

        Task<Result<List<Permission>>> GetByRoleIdAsync(int roleId);
        Task<Result<List<Permission>>> GetPagedByRoleIdAsync(int roleId, int page, int pageSize);
        Task<Result<int>> CountByRoleIdAsync(int roleId);
    }
}
