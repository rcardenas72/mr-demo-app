using DemoApp.Web.Models;
using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Services.Utils;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Services
{
    public class MenuService : BaseService<MenuService>, IMenuService
    {
        private readonly IMenuRepository _repository;

        public MenuService(
            IMenuRepository repository,
            ICurrentUserService currentUserService,
            ILogger<MenuService> logger)
            : base(logger, currentUserService)
        {
            _repository = repository;
        }

        public async Task<Result<List<Menu>>> GetAllAsync()
            => await ExecuteAsync(() => _repository.GetAllAsync(), "Error al obtener todos los menús", "Error al obtener los menús.");

        public async Task<Result<Menu?>> GetByIdAsync(int id)
            => await ExecuteAsync(() => _repository.GetByIdAsync(id), $"Error al obtener menú con ID {id}", "Error al obtener el menú.");

        public async Task<Result> CreateAsync(Menu menu)
        {
            var validation = await ValidateAsync(menu, isEdit: false);
            if (!validation.IsSuccess)
                return validation;

            menu.ControllerName = menu.ControllerName!.Trim();
            menu.InsUser = GetUsername();

            return await ExecuteAsync(() => _repository.CreateAsync(menu), "Error al crear menú", "Error inesperado al crear el menú.");
        }

        public async Task<Result> UpdateAsync(Menu menu)
        {
            var validation = await ValidateAsync(menu, isEdit: true);
            if (!validation.IsSuccess)
                return validation;

            var existingResult = await GetByIdAsync(menu.MenuId);
            if (!existingResult.IsSuccess || existingResult.Data == null)
                return Result.Failure("El menú no existe.");

            var existing = existingResult.Data;
            existing.ControllerName = menu.ControllerName!.Trim();
            existing.EntityName = menu.EntityName!.Trim();
            existing.UpdDate = DateTime.UtcNow;
            existing.UpdUser = GetUsername();

            return await ExecuteAsync(() => _repository.UpdateAsync(existing), $"Error al actualizar menú con ID {menu.MenuId}", "Error inesperado al actualizar el menú.");
        }

        private async Task<Result> ValidateAsync(Menu menu, bool isEdit)
        {
            if (string.IsNullOrWhiteSpace(menu.ControllerName))
                return Result.Failure("El nombre no puede estar vacío.");

            if (menu.ControllerName.Trim().Length > 50)
                return Result.Failure("El nombre no puede tener más de 50 caracteres.");

            try
            {
                bool exists = isEdit
                    ? await _repository.ExistsByNameAsync(menu.ControllerName, menu.MenuId)
                    : await _repository.ExistsByNameAsync(menu.ControllerName);

                if (exists)
                    return Result.Failure("Ya existe un menú con ese controlador.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                LogError("Error al validar el menú", ex);
                return Result.Failure("Error inesperado al validar el menú.");
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return Result.Success();
            }
            catch (DbUpdateException ex)
            {
                LogError($"Error de integridad referencial al eliminar menú con ID {id}", ex);
                return Result.Failure("No se puede eliminar este registro porque está asociado a otros datos.");
            }
            catch (Exception ex)
            {
                LogError($"Error inesperado al eliminar menú con ID {id}", ex);
                return Result.Failure("Error inesperado al eliminar el menú.");
            }
        }

        public async Task<Result<List<Menu>>> GetPagedAsync(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            return await ExecuteAsync(() => _repository.GetPagedAsync(page, pageSize), $"Error al obtener paginación de menús - Page: {page}, PageSize: {pageSize}", "Error al obtener los menús paginados.");
        }

        public async Task<Result<int>> CountAsync()
            => await ExecuteAsync(() => _repository.CountAsync(), "Error al contar los menús", "Error al contar los menús.");
    }
}
