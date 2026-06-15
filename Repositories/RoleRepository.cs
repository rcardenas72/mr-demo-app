using DemoApp.Web.Data;
using DemoApp.Web.Models;
using DemoApp.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllAsync() => await _context.Roles.AsNoTracking().ToListAsync();

        public async Task<List<Role>> GetActiveAsync() => await _context.Roles.Where(r => r.IsActive).AsNoTracking().ToListAsync();

        public async Task<Role?> GetByIdAsync(int id) => await _context.Roles.FindAsync(id);

        public async Task<bool> IsActiveAsync(int id) => await _context.Roles.AnyAsync(r => r.RoleId == id && r.IsActive);

        public async Task<bool> HasActiveDependentsAsync(int id) => await _context.Users.AnyAsync(u => u.RoleId == id && u.IsActive) || await _context.Permissions.AnyAsync(p => p.RoleId == id && p.IsActive);

        public async Task CreateAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            var existing = await _context.Roles.FindAsync(role.RoleId);
            if (existing == null) return;

            existing.RoleName = role.RoleName;
            existing.IsActive = role.IsActive;
            existing.UpdUser = role.UpdUser;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Roles
                .AnyAsync(x => x.RoleName.ToUpper().Trim() == name.ToUpper().Trim());
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeId)
        {
            return await _context.Roles
                .AnyAsync(x => x.RoleName.ToUpper().Trim() == name.ToUpper().Trim()
                            && x.RoleId != excludeId);
        }
    }

}
