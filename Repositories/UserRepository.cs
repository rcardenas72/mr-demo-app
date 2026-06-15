using DemoApp.Web.Data;
using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;
using DemoApp.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppUser>> GetAllAsync() => await _context.Users.AsNoTracking().ToListAsync();

        public async Task<AppUser?> GetByIdAsync(int id) => await _context.Users.FindAsync(id);

        public async Task<AppUser?> GetUserInfoAsync(string userName)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Role) 
                .FirstOrDefaultAsync(x => x.UserName == userName);
        }

        public async Task CreateAsync(AppUser user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AppUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AppUser>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            return await _context.Users
                .AsNoTracking()
                .OrderBy(c => c.UserName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<(List<AppUser> Users, int TotalCount)> GetFilteredAsync(UserFilterDto filter)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(u =>
                    u.UserName.Contains(filter.SearchTerm) ||
                    u.FirstName.Contains(filter.SearchTerm) ||
                    u.LastName.Contains(filter.SearchTerm) ||
                    u.Email.Contains(filter.SearchTerm));
            }

            if (filter.RoleId.HasValue)
                query = query.Where(u => u.RoleId == filter.RoleId.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            var total = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (users, total);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Users
                .AnyAsync(x => x.UserName.ToUpper().Trim() == name.ToUpper().Trim());
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeId)
        {
            return await _context.Users
                .AnyAsync(x => x.UserName.ToUpper().Trim() == name.ToUpper().Trim()
                            && x.UserId != excludeId);
        }

        public async Task<List<AppUser>> GetActiveAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();
        }
    }

}
