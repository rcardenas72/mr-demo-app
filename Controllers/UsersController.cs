using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DemoApp.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using DemoApp.Web.Mappings;
using DemoApp.Web.Models.DTOs;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Filters;
using DemoApp.Web.ViewModels;

namespace DemoApp.Web.Controllers
{
    [Authorize]
    [PermissionFilter(OperationType.Consultar)]
    public class UsersController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly AppMapper _mapper;

        public UsersController(
            IRoleService roleService,
            IUserService userService,
            AppMapper mapper)
        {
            _roleService = roleService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int? roleId, bool? isActive, int page = 1, int pageSize = 7)
        {
            var filter = new UserFilterDto
            {
                SearchTerm = searchTerm,
                RoleId = roleId,
                IsActive = isActive,
                Page = page,
                PageSize = pageSize
            };

            var filterResult = await _userService.GetFilteredAsync(filter);
            if (!filterResult.IsSuccess)
                return View("Error", filterResult.ErrorMessage);

            var rolesResult = await _roleService.GetActiveAsync();
            if (!rolesResult.IsSuccess)
                return View("Error", rolesResult.ErrorMessage);

            var paged = filterResult.Data!;
            var roles = rolesResult.Data!;

            var viewModel = new UserFilterViewModel
            {
                SearchTerm = searchTerm,
                RoleId = roleId,
                IsActive = isActive,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = paged.TotalPages,
                Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName
                }).ToList(),
                Users = paged.Items.Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    RoleName = u.Role?.RoleName,
                    IsActive = u.IsActive,
                    IsAdmin = u.IsAdmin
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        [PermissionFilter(OperationType.Agregar)]
        public async Task<IActionResult> Create()
        {
            var model = new UserFormViewModel
            {
                IsActive = true,
                Roles = await GetRoleSelectListAsync()
            };

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Agregar)]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await GetRoleSelectListAsync();
                return View("Form", model);
            }

            var result = await _userService.CreateAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Error al crear el usuario.");
                model.Roles = await GetRoleSelectListAsync();
                return View("Form", model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(int id)
        {
            var userResult = await _userService.GetByIdAsync(id);
            if (!userResult.IsSuccess)
                return View("Error", userResult.ErrorMessage);

            if (userResult.Data == null)
                return NotFound();

            var model = _mapper.EntityToUserForm(userResult.Data);
            model.Roles = await GetRoleSelectListAsync();

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await GetRoleSelectListAsync();
                return View("Form", model);
            }

            var result = await _userService.UpdateAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Error al actualizar el usuario.");
                model.Roles = await GetRoleSelectListAsync();
                return View("Form", model);
            }

            return RedirectToAction("Index");
        }

        private async Task<List<SelectListItem>> GetRoleSelectListAsync()
        {
            var rolesResult = await _roleService.GetActiveAsync();
            if (!rolesResult.IsSuccess)
                return new List<SelectListItem>();

            return rolesResult.Data!.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = r.RoleName
            }).ToList();
        }
    }
}
