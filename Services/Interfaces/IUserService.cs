using DemoApp.Web.ViewModels;
using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;
using DemoApp.Web.Services.Utils;


namespace DemoApp.Web.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<List<AppUser>>> GetAllAsync();
        Task<Result<AppUser?>> GetByIdAsync(int id);
        Task<Result> CreateAsync(UserFormViewModel user);
        Task<Result> UpdateAsync(UserFormViewModel user);
        Task<Result> DeleteAsync(int id);
        Task<Result<List<AppUser>>> GetPagedAsync(int page, int pageSize);
        Task<Result<int>> CountAsync();
        Task<Result<PagedResult<AppUser>>> GetFilteredAsync(UserFilterDto filter);
        Task<Result<AppUser?>> GetByUserNameAsync(string userName);
        Task<Result<List<AppUser>>> GetActiveAsync();
        Task<Result<AppUser?>> GetProfileAsync(string username);
    }
}
