using DemoApp.Web.Models;
using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Services.Utils;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Services
{
    public class RoleService : BaseService<RoleService>, IRoleService
    {
        private readonly IRoleRepository _repository;

        public RoleService(
            IRoleRepository repository,
            ICurrentUserService currentUserService,
            ILogger<RoleService> logger)
            : base(logger, currentUserService)
        {
            _repository = repository;
        }

        public async Task<Result<List<Role>>> GetAllAsync()
            => await ExecuteAsync(() => _repository.GetAllAsync(), "Error al obtener todos los roles", "Error al obtener los roles.");

        public async Task<Result<List<Role>>> GetActiveAsync()
            => await ExecuteAsync(() => _repository.GetActiveAsync(), "Error al obtener roles activos", "Error al obtener los roles activos.");

        public async Task<Result<Role?>> GetByIdAsync(int id)
            => await ExecuteAsync(() => _repository.GetByIdAsync(id), $"Error al obtener el rol con ID {id}", "Error al obtener el rol.");

        public async Task<Result> CreateAsync(Role role)
        {
            var validation = await ValidateAsync(role, isEdit: false);
            if (!validation.IsSuccess)
                return validation;

            role.RoleName = role.RoleName!.Trim();
            role.InsUser = GetUsername();

            return await ExecuteAsync(() => _repository.CreateAsync(role), "Error al crear rol", "Error inesperado al crear el rol.");
        }

        public async Task<Result> UpdateAsync(Role role)
        {
            var validation = await ValidateAsync(role, isEdit: true);
            if (!validation.IsSuccess)
                return validation;

            var existingResult = await GetByIdAsync(role.RoleId);
            if (!existingResult.IsSuccess || existingResult.Data == null)
                return Result.Failure("El rol no existe.");

            var existing = existingResult.Data;

            if (existing.IsActive && !role.IsActive)
            {
                if (await _repository.HasActiveDependentsAsync(role.RoleId))
                    return Result.Failure("No se puede desactivar el rol porque tiene usuarios o permisos activos asociados.");
            }

            existing.RoleName = role.RoleName!.Trim();
            existing.IsActive = role.IsActive;
            existing.UpdUser = GetUsername();

            return await ExecuteAsync(() => _repository.UpdateAsync(existing), $"Error al actualizar rol ID {role.RoleId}", "Error inesperado al actualizar el rol.");
        }

        public async Task<Result> DeleteAsync(int id)
        {
            if (await _repository.HasActiveDependentsAsync(id))
                return Result.Failure("No se puede eliminar el rol porque tiene usuarios o permisos asociados.");

            try
            {
                await _repository.DeleteAsync(id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                LogError($"Error inesperado al eliminar rol ID {id}", ex);
                return Result.Failure("Error inesperado al eliminar.");
            }
        }

        private async Task<Result> ValidateAsync(Role role, bool isEdit)
        {
            if (string.IsNullOrWhiteSpace(role.RoleName))
                return Result.Failure("El nombre del rol no puede estar vacío.");

            if (role.RoleName.Trim().Length > 50)
                return Result.Failure("El nombre no puede tener más de 50 caracteres.");

            try
            {
                bool exists = isEdit
                    ? await _repository.ExistsByNameAsync(role.RoleName, role.RoleId)
                    : await _repository.ExistsByNameAsync(role.RoleName);

                if (exists)
                    return Result.Failure("Ya existe un rol con ese nombre.");
            }
            catch (Exception ex)
            {
                LogError("Error al validar rol", ex);
                return Result.Failure("Error inesperado al validar el rol.");
            }

            return Result.Success();
        }
    }
}
