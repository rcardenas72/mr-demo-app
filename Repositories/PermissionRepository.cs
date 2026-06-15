using DemoApp.Web.Data;
using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetPermissions(string username)
        {
            return await _context.Permissions
                .AsNoTracking()
                .Include(p => p.Menu)
                .Include(p => p.Role)
                .Where(p => p.Role != null && p.Role.Users.Any(u => u.UserName == username))
                .ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions
                .Include(p => p.Role)
                .Include(p => p.Menu)
                .FirstOrDefaultAsync(p => p.PermissionId == id);
        }

        public async Task CreateAsync(Permission permission)
        {
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Permission permission)
        {
                _context.Permissions.Update(permission);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission != null)
            {
                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetAccessibleMenuOptionsByUsernameAsync(string username)
        {
            var roleId = await _context.Users
                .Where(u => u.UserName == username)
                .Select(u => u.RoleId)
                .FirstOrDefaultAsync();

            if (roleId == 0)
                return new List<string>();

            return await _context.Permissions
                .Where(p => p.RoleId == roleId && p.IsActive)
                .Select(p => p.Menu.ControllerName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<Permission>> GetByRoleIdAsync(int roleId)
        {
            return await _context.Permissions
                .AsNoTracking()
                .Include(p => p.Menu)
                .Include(p => p.Role)
                .Where(p => p.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetPagedByRoleIdAsync(int roleId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            return await _context.Permissions
                .AsNoTracking()
                .Include(p => p.Menu)
                .Include(p => p.Role)
                .Where(p => p.RoleId == roleId)
                .OrderBy(p => p.Menu.MenuName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByRoleIdAsync(int roleId)
        {
            return await _context.Permissions
                .Where(p => p.RoleId == roleId)
                .CountAsync();
        }

        public async Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType)
        {
            return await _context.Permissions.AnyAsync(p =>
                p.RoleId == roleId &&
                p.MenuId == menuId &&
                p.OperationType == operationType);
        }

        public async Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType, int excludeId)
        {
            return await _context.Permissions.AnyAsync(p =>
                p.RoleId == roleId &&
                p.MenuId == menuId &&
                p.OperationType == operationType &&
                p.PermissionId != excludeId);
        }
    }
}
