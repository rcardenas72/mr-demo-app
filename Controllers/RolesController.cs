using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Filters;

namespace DemoApp.Web.Controllers
{
    [Authorize]
    [PermissionFilter(OperationType.Consultar)]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _roleService.GetAllAsync();
            if (!result.IsSuccess)
                return View("Error", result.ErrorMessage);

            return View(result.Data);
        }

        [HttpGet]
        [PermissionFilter(OperationType.Agregar)]
        public IActionResult Create()
        {
            return View("Form", new Role { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Agregar)]
        public async Task<IActionResult> Create(Role role)
        {
            if (!ModelState.IsValid)
                return View("Form", role);

            var result = await _roleService.CreateAsync(role);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Error al crear.");
                return View("Form", role);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _roleService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return View("Error", result.ErrorMessage);

            if (result.Data == null)
                return NotFound();

            return View("Form", result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(Role role)
        {
            if (!ModelState.IsValid)
                return View("Form", role);

            var result = await _roleService.UpdateAsync(role);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Error al actualizar.");
                return View("Form", role);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return View("Error", result.ErrorMessage);

            if (result.Data == null)
                return NotFound();

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _roleService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Error al eliminar.");
                var itemResult = await _roleService.GetByIdAsync(id);
                return View("Delete", itemResult.IsSuccess ? itemResult.Data : null);
            }

            return RedirectToAction("Index");
        }
    }
}
