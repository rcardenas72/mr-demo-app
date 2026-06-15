using DemoApp.Web.Data;
using DemoApp.Web.Models;
using DemoApp.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;

        public MenuRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Menu>> GetAllAsync() => await _context.Menus.AsNoTracking().ToListAsync();

        public async Task<Menu?> GetByIdAsync(int id) => await _context.Menus.FindAsync(id);

        public async Task CreateAsync(Menu menu)
        {
            menu.InsDate = DateTime.UtcNow;
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Menu menu)
        {
            _context.Menus.Update(menu);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return;

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Menu>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            return await _context.Menus
                .AsNoTracking()
                .OrderBy(c => c.MenuName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Menus.CountAsync();
        }

        public async Task<bool> ExistsByNameAsync(string controlador)
        {
            return await _context.Menus
                .AnyAsync(x => x.ControllerName.ToUpper().Trim() == controlador.ToUpper().Trim());
        }

        public async Task<bool> ExistsByNameAsync(string controlador, int excludeId)
        {
            return await _context.Menus
                .AnyAsync(x => x.ControllerName.ToUpper().Trim() == controlador.ToUpper().Trim()
                            && x.MenuId != excludeId);
        }
    }
}

