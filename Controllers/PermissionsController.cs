using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using DemoApp.Web.Mappings;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Filters;
using DemoApp.Web.ViewModels;
using DemoApp.Web.Services.Utils;

namespace DemoApp.Web.Controllers
{
    [Authorize]
    [PermissionFilter(OperationType.Consultar)]
    public class PermissionsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly IMenuService _menuService;
        private readonly AppMapper _mapper;

        public PermissionsController(
            IPermissionService permissionService,
            IRoleService roleService,
            IMenuService menuService,
            AppMapper mapper)
        {
            _permissionService = permissionService;
            _roleService = roleService;
            _menuService = menuService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? roleId, int page = 1, int pageSize = 7)
        {
            var rolesResult = await GetRolesAsync();
            if (!rolesResult.IsSuccess)
                return View("Error", rolesResult.ErrorMessage);

            var model = new PermissionIndexViewModel
            {
                SelectedRoleId = roleId,
                Roles = rolesResult.Data!
            };

            if (roleId == null)
            {
                model.PagedPermissions = new PagedResultViewModel<Permission>
                {
                    Items = Enumerable.Empty<Permission>()
                };
                return View(model);
            }

            var countResult = await _permissionService.CountByRoleIdAsync(roleId.Value);
            if (!countResult.IsSuccess)
                return View("Error", countResult.ErrorMessage);

            var totalPages = (int)Math.Ceiling((double)countResult.Data / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var pagedResult = await _permissionService.GetPagedByRoleIdAsync(roleId.Value, page, pageSize);
            if (!pagedResult.IsSuccess)
                return View("Error", pagedResult.ErrorMessage);

            model.PagedPermissions = new PagedResultViewModel<Permission>
            {
                Items = pagedResult.Data!,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }

        [HttpGet]
        [PermissionFilter(OperationType.Agregar)]
        public async Task<IActionResult> Create(int? roleId)
        {
            var rolesResult = await GetRolesAsync();
            var menusResult = await GetMenusAsync();

            var model = new PermissionFormViewModel
            {
                Roles = rolesResult.IsSuccess ? rolesResult.Data! : new List<SelectListItem>(),
                Menus = menusResult.IsSuccess ? menusResult.Data! : new List<SelectListItem>(),
                Operations = GetOperations(),
                RoleId = roleId ?? 0,
                IsActive = true
            };

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Agregar)]
        public async Task<IActionResult> Create(PermissionFormViewModel model)
        {
            if (!ModelState.IsValid)
                return await ReturnPermissionFormViewAsync(model);

            var result = await _permissionService.CreateAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Ocurrió un error al crear el permiso.");
                return await ReturnPermissionFormViewAsync(model);
            }

            return RedirectToAction(nameof(Index), new { roleId = model.RoleId });
        }

        [HttpGet]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(int id)
        {
            var permissionResult = await _permissionService.GetByIdAsync(id);
            if (!permissionResult.IsSuccess)
                return View("Error", permissionResult.ErrorMessage);

            if (permissionResult.Data == null)
                return NotFound();

            var model = _mapper.EntityToPermissionForm(permissionResult.Data);
            var rolesResult = await GetRolesAsync();
            var menusResult = await GetMenusAsync();
            model.Roles = rolesResult.IsSuccess ? rolesResult.Data! : new List<SelectListItem>();
            model.Menus = menusResult.IsSuccess ? menusResult.Data! : new List<SelectListItem>();
            model.Operations = GetOperations();

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(PermissionFormViewModel model)
        {
            if (!ModelState.IsValid)
                return await ReturnPermissionFormViewAsync(model);

            var result = await _permissionService.UpdateAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Ocurrió un error inesperado.");
                return await ReturnPermissionFormViewAsync(model);
            }

            return RedirectToAction(nameof(Index), new { roleId = model.RoleId });
        }

        [HttpGet]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> Delete(int id)
        {
            var permissionResult = await _permissionService.GetByIdAsync(id);
            if (!permissionResult.IsSuccess)
                return View("Error", permissionResult.ErrorMessage);

            if (permissionResult.Data == null)
                return NotFound();

            var model = _mapper.EntityToPermissionForm(permissionResult.Data);
            model.RoleName = permissionResult.Data.Role?.RoleName;
            model.MenuName = permissionResult.Data.Menu?.MenuName;

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> DeleteConfirmed(int id, int roleId)
        {
            var result = await _permissionService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo eliminar el permiso.");

                var itemResult = await _permissionService.GetByIdAsync(id);
                if (itemResult.IsSuccess && itemResult.Data != null)
                {
                    var model = _mapper.EntityToPermissionForm(itemResult.Data);
                    model.RoleName = itemResult.Data.Role?.RoleName;
                    model.MenuName = itemResult.Data.Menu?.MenuName;
                    return View("Delete", model);
                }

                return RedirectToAction(nameof(Index), new { roleId });
            }

            return RedirectToAction(nameof(Index), new { roleId });
        }

        private async Task<Result<List<SelectListItem>>> GetRolesAsync()
        {
            var rolesResult = await _roleService.GetActiveAsync();
            if (!rolesResult.IsSuccess)
                return Result<List<SelectListItem>>.Failure("Error al cargar roles.");

            return Result<List<SelectListItem>>.Success(rolesResult.Data!.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = r.RoleName
            }).ToList());
        }

        private async Task<Result<List<SelectListItem>>> GetMenusAsync()
        {
            var menusResult = await _menuService.GetAllAsync();
            if (!menusResult.IsSuccess)
                return Result<List<SelectListItem>>.Failure("Error al cargar menús.");

            return Result<List<SelectListItem>>.Success(menusResult.Data!.Select(m => new SelectListItem
            {
                Value = m.MenuId.ToString(),
                Text = m.MenuName
            }).ToList());
        }

        private List<SelectListItem> GetOperations()
        {
            return OperationTypeHelper
                .GetOperationList()
                .Select(o => new SelectListItem { Text = o.Key, Value = o.Value })
                .ToList();
        }

        private async Task<IActionResult> ReturnPermissionFormViewAsync(PermissionFormViewModel model, string viewName = "Form")
        {
            var rolesResult = await GetRolesAsync();
            var menusResult = await GetMenusAsync();

            model.Roles = rolesResult.IsSuccess ? rolesResult.Data! : new List<SelectListItem>();
            model.Menus = menusResult.IsSuccess ? menusResult.Data! : new List<SelectListItem>();
            model.Operations = GetOperations();

            return View(viewName, model);
        }
    }
}
