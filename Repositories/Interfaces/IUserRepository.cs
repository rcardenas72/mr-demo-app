using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;

namespace DemoApp.Web.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<AppUser>> GetAllAsync();
        Task<AppUser?> GetByIdAsync(int id);
        Task<AppUser?> GetUserInfoAsync(string userName);
        Task CreateAsync(AppUser user);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(int id);
        Task<(List<AppUser> Users, int TotalCount)> GetFilteredAsync(UserFilterDto filter);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int excludeId);
        Task<List<AppUser>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> CountAsync();
        Task<List<AppUser>> GetActiveAsync();
    }
}
