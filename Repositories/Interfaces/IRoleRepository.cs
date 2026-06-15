using DemoApp.Web.Models;

namespace DemoApp.Web.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllAsync();
        Task<List<Role>> GetActiveAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<bool> IsActiveAsync(int id);
        Task<bool> HasActiveDependentsAsync(int id);
        Task CreateAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int excludeId);
    }
}
