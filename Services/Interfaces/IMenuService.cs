using DemoApp.Web.Models;
using DemoApp.Web.Services.Utils;

namespace DemoApp.Web.Services.Interfaces
{
    public interface IMenuService
    {
        Task<Result<List<Menu>>> GetAllAsync();
        Task<Result<Menu?>> GetByIdAsync(int id);
        Task<Result> CreateAsync(Menu menu);
        Task<Result> UpdateAsync(Menu menu);
        Task<Result> DeleteAsync(int id);
        Task<Result<List<Menu>>> GetPagedAsync(int page, int pageSize);
        Task<Result<int>> CountAsync();
    }
}
