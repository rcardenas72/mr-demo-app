using DemoApp.Web.Mappings;
using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Services.Utils;
using DemoApp.Web.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace DemoApp.Web.Services
{
    public class PermissionService : BaseService<PermissionService>, IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMemoryCache _cache;
        private readonly AppMapper _mapper;

        public PermissionService(
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IMemoryCache cache,
            AppMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<PermissionService> logger)
            : base(logger, currentUserService)
        {
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<Result<List<string>>> GetAccessibleMenuOptionsAsync(string userName)
        {
            var cacheKey = $"{userName}_AccessibleMenus";

            if (_cache.TryGetValue(cacheKey, out List<string>? cached))
                return Result<List<string>>.Success(cached!);

            return await ExecuteAsync(async () =>
            {
                var permissions = await _permissionRepository.GetAccessibleMenuOptionsByUsernameAsync(userName);
                _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));
                return permissions;
            }, $"Error al obtener accesos de menú para el usuario {userName}", "Error al obtener accesos de menú.");
        }

        public async Task<Result<List<Permission>>> GetByRoleIdAsync(int roleId)
            => await ExecuteAsync(() => _permissionRepository.GetByRoleIdAsync(roleId), $"Error al obtener permisos por rol ID {roleId}", "Error al obtener los permisos.");

        public async Task<Result<List<Permission>>> GetPagedByRoleIdAsync(int roleId, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            return await ExecuteAsync(() => _permissionRepository.GetPagedByRoleIdAsync(roleId, page, pageSize), $"Error al paginar permisos del rol {roleId}", "Error al obtener los permisos paginados.");
        }

        public async Task<Result<int>> CountByRoleIdAsync(int roleId)
            => await ExecuteAsync(() => _permissionRepository.CountByRoleIdAsync(roleId), $"Error al contar permisos del rol {roleId}", "Error al contar los permisos.");

        public async Task<Result<Permission?>> GetByIdAsync(int id)
            => await ExecuteAsync(() => _permissionRepository.GetByIdAsync(id), $"Error al obtener permiso con ID {id}", "Error al obtener el permiso.");

        public async Task<Result> CreateAsync(PermissionFormViewModel model)
        {
            if (!await _roleRepository.IsActiveAsync(model.RoleId))
                return Result.Failure("No se puede asignar un permiso a un rol inactivo.");

            var permission = _mapper.PermissionFormToEntity(model);

            if (!await ValidateAsync(permission))
                return Result.Failure("Ya existe un permiso con este rol, menú y tipo de operación.");

            permission.InsUser = GetUsername();

            return await ExecuteAsync(() => _permissionRepository.CreateAsync(permission), "Error al crear permiso", "Error inesperado al crear el permiso.");
        }

        public async Task<Result> UpdateAsync(PermissionFormViewModel model)
        {
            if (!await _roleRepository.IsActiveAsync(model.RoleId))
                return Result.Failure("No se puede asignar un permiso a un rol inactivo.");

            var existingResult = await GetByIdAsync(model.PermissionId!.Value);
            if (!existingResult.IsSuccess || existingResult.Data == null)
                return Result.Failure("El permiso no existe.");

            var existing = existingResult.Data;

            if (!await ValidateAsync(model, existing))
                return Result.Failure("Ya existe un permiso con esta combinación de rol, menú y operación.");

            _mapper.Map(model, existing);
            existing.UpdUser = GetUsername();

            return await ExecuteAsync(() => _permissionRepository.UpdateAsync(existing), $"Error al actualizar permiso con ID {model.PermissionId}", "Error inesperado al actualizar el permiso.");
        }

        public async Task<Result> DeleteAsync(int id)
            => await ExecuteAsync(() => _permissionRepository.DeleteAsync(id), $"Error al eliminar permiso con ID {id}", "Error inesperado al eliminar el permiso.");

        public async Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType)
        {
            return await _permissionRepository.ExistsAsync(roleId, menuId, operationType);
        }

        public async Task<bool> ExistsAsync(int roleId, int menuId, OperationType operationType, int excludeId)
        {
            return await _permissionRepository.ExistsAsync(roleId, menuId, operationType, excludeId);
        }

        private async Task<bool> ValidateAsync(Permission permission, int? excludeId = null)
        {
            try
            {
                if (excludeId.HasValue)
                {
                    return !await _permissionRepository.ExistsAsync(
                        permission.RoleId, permission.MenuId, permission.OperationType, excludeId.Value);
                }

                return !await _permissionRepository.ExistsAsync(
                    permission.RoleId, permission.MenuId, permission.OperationType);
            }
            catch (Exception ex)
            {
                LogError("Error al validar permiso", ex);
                return false;
            }
        }

        private async Task<bool> ValidateAsync(PermissionFormViewModel model, Permission existing)
        {
            try
            {
                var roleChanged = model.RoleId != existing.RoleId;
                var menuChanged = model.MenuId != existing.MenuId;
                var operationChanged = model.OperationType != existing.OperationType;

                if (!roleChanged && !menuChanged && !operationChanged)
                    return true;

                return !await _permissionRepository.ExistsAsync(
                    model.RoleId, model.MenuId, model.OperationType, existing.PermissionId);
            }
            catch (Exception ex)
            {
                LogError("Error al validar permiso", ex);
                return false;
            }
        }
    }
}
