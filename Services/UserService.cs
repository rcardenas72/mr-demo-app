using DemoApp.Web.Mappings;
using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;
using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Services.Utils;
using DemoApp.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Services
{
    public class UserService : BaseService<UserService>, IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IRoleRepository _roleRepository;
        private readonly AppMapper _mapper;

        public UserService(
            IUserRepository repository,
            IRoleRepository roleRepository,
            ICurrentUserService currentUserService,
            AppMapper mapper,
            ILogger<UserService> logger)
            : base(logger, currentUserService)
        {
            _repository = repository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<AppUser>>> GetAllAsync()
            => await ExecuteAsync(() => _repository.GetAllAsync(), "Error al obtener la lista de usuarios", "Error al obtener la lista de usuarios.");

        public async Task<Result<AppUser?>> GetByIdAsync(int id)
            => await ExecuteAsync(() => _repository.GetByIdAsync(id), $"Error al obtener el usuario con ID {id}", "Error al obtener el usuario.");

        public async Task<Result> CreateAsync(UserFormViewModel user)
        {
            var validationResult = await ValidateUserAsync(user);
            if (!validationResult.IsSuccess)
                return validationResult;

            var entity = _mapper.UserFormToEntity(user);
            NormalizeUser(entity);
            return await ExecuteAsync(() => _repository.CreateAsync(entity), "Error al crear usuario", "Error inesperado al crear el usuario.");
        }

        public async Task<Result> UpdateAsync(UserFormViewModel user)
        {
            var validationResult = await ValidateUserAsync(user, isEdit: true);
            if (!validationResult.IsSuccess)
                return validationResult;

            var existingResult = await GetByIdAsync(user.UserId);
            if (!existingResult.IsSuccess || existingResult.Data == null)
                return Result.Failure("Usuario no encontrado.");

            var existing = existingResult.Data;
            NormalizeUser(existing);

            _mapper.Map(user, existing);
            return await ExecuteAsync(() => _repository.UpdateAsync(existing), $"Error al actualizar usuario ID {user.UserId}", "Error inesperado al actualizar el usuario.");
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
                LogError($"Error de integridad referencial al eliminar usuario ID {id}", ex);
                return Result.Failure("No se puede eliminar este usuario porque está asociado a otros datos.");
            }
            catch (Exception ex)
            {
                LogError($"Error inesperado al eliminar usuario ID {id}", ex);
                return Result.Failure("Error inesperado al eliminar.");
            }
        }

        public async Task<Result<List<AppUser>>> GetPagedAsync(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            return await ExecuteAsync(() => _repository.GetPagedAsync(page, pageSize), "Error al obtener usuarios paginados", "Error al obtener usuarios paginados.");
        }

        public async Task<Result<int>> CountAsync()
            => await ExecuteAsync(() => _repository.CountAsync(), "Error al contar usuarios", "Error al contar usuarios.");

        public async Task<Result<PagedResult<AppUser>>> GetFilteredAsync(UserFilterDto filter)
            => await ExecuteAsync(async () =>
            {
                var (users, total) = await _repository.GetFilteredAsync(filter);
                return new PagedResult<AppUser>
                {
                    Items = users,
                    TotalCount = total,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }, "Error al filtrar usuarios", "Error al filtrar usuarios.");

        private async Task<Result> ValidateUserAsync(UserFormViewModel user, bool isEdit = false)
        {
            if (string.IsNullOrWhiteSpace(user.FirstName))
                return Result.Failure("El nombre no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(user.LastName))
                return Result.Failure("El apellido no puede estar vacío.");

            if (user.FirstName.Trim().Length > 50)
                return Result.Failure("El nombre no puede tener más de 50 caracteres.");

            if (user.LastName.Trim().Length > 50)
                return Result.Failure("El apellido no puede tener más de 50 caracteres.");

            if (user.RoleId <= 0)
                return Result.Failure("Debe seleccionar un rol para el usuario.");

            try
            {
                if (!await _roleRepository.IsActiveAsync(user.RoleId))
                    return Result.Failure("El rol seleccionado no está activo.");

                bool exists = isEdit
                    ? await _repository.ExistsByNameAsync(user.UserName, user.UserId)
                    : await _repository.ExistsByNameAsync(user.UserName);

                if (exists)
                    return Result.Failure("Ya existe un usuario con ese nombre de usuario.");
            }
            catch (Exception ex)
            {
                LogError("Error al validar usuario", ex);
                return Result.Failure("Error inesperado al validar el usuario.");
            }

            return Result.Success();
        }

        private void NormalizeUser(AppUser user)
        {
            user.UserName = user.UserName.Trim();
            user.FirstName = user.FirstName.Trim();
            user.LastName = user.LastName.Trim();
        }

        public async Task<Result<AppUser?>> GetByUserNameAsync(string userName)
            => await ExecuteAsync(() => _repository.GetUserInfoAsync(userName), $"Error al obtener usuario por nombre {userName}", "Error al obtener el usuario.");

        public async Task<Result<List<AppUser>>> GetActiveAsync()
            => await ExecuteAsync(() => _repository.GetActiveAsync(), "Error al obtener los usuarios activos", "Error al obtener usuarios activos.");

        public async Task<Result<AppUser?>> GetProfileAsync(string username)
            => await ExecuteAsync(() => _repository.GetUserInfoAsync(username), $"Error al obtener perfil de {username}", "Error al obtener el perfil del usuario.");

    }
}
