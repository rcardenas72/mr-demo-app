using DemoApp.Web.Models;

namespace DemoApp.Web.Repositories.Interfaces
{
    public interface IMenuRepository
    {
        Task<List<Menu>> GetAllAsync();
        Task<Menu?> GetByIdAsync(int id);
        Task CreateAsync(Menu menu);
        Task UpdateAsync(Menu menu);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string controlador);
        Task<bool> ExistsByNameAsync(string controlador, int excludeId);
        Task<List<Menu>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> CountAsync();
    }
}
