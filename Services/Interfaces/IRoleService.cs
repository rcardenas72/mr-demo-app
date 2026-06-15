using DemoApp.Web.Models;
using DemoApp.Web.Services.Utils;

namespace DemoApp.Web.Services.Interfaces
{
    public interface IRoleService
    {
        Task<Result<List<Role>>> GetAllAsync();
        Task<Result<List<Role>>> GetActiveAsync();
        Task<Result<Role?>> GetByIdAsync(int id);
        Task<Result> CreateAsync(Role role);
        Task<Result> UpdateAsync(Role role);
        Task<Result> DeleteAsync(int id);
    }
}
