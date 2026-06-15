using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.ViewModels;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Filters;

namespace DemoApp.Web.Controllers
{
    [Authorize]
    [PermissionFilter(OperationType.Consultar)]
    public class MenusController : Controller
    {
        private readonly IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 7)
        {
            var countResult = await _menuService.CountAsync();
            if (!countResult.IsSuccess)
                return View("Error", countResult.ErrorMessage);

            if (countResult.Data == 0)
            {
                return View(new PagedResultViewModel<Menu>
                {
                    Items = Enumerable.Empty<Menu>()
                });
            }

            var totalPages = (int)Math.Ceiling(countResult.Data / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var menusResult = await _menuService.GetPagedAsync(page, pageSize);
            if (!menusResult.IsSuccess)
                return View("Error", menusResult.ErrorMessage);

            var viewModel = new PagedResultViewModel<Menu>
            {
                Items = menusResult.Data!,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        [PermissionFilter(OperationType.Agregar)]
        public IActionResult Create()
        {
            return View("Form", new Menu());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Agregar)]
        public async Task<IActionResult> Create(Menu menu)
        {
            if (!ModelState.IsValid)
                return View("Form", menu);

            var result = await _menuService.CreateAsync(menu);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo crear el menú.");
                return View("Form", menu);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _menuService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return View("Error", result.ErrorMessage);

            if (result.Data == null)
                return NotFound();

            return View("Form", result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(Menu menu)
        {
            if (!ModelState.IsValid)
                return View("Form", menu);

            var result = await _menuService.UpdateAsync(menu);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo actualizar el menú.");
                return View("Form", menu);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _menuService.GetByIdAsync(id);
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
            var result = await _menuService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo eliminar el menú.");
                var itemResult = await _menuService.GetByIdAsync(id);
                return View("Delete", itemResult.IsSuccess ? itemResult.Data : null);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
