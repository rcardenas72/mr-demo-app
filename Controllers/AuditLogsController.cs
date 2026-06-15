using DemoApp.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DemoApp.Web.ViewModels;
using DemoApp.Web.Models.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using DemoApp.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using DemoApp.Web.Models;
using DemoApp.Web.Helpers;
using DemoApp.Web.Filters;

namespace DemoApp.Web.Controllers
{
    [Authorize]
    [PermissionFilter(OperationType.Consultar)]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? performedBy,
            string? entityName,
            string? operationType,
            DateTime? performedFrom,
            DateTime? performedTo,
            int page = 1,
            int pageSize = 8)
        {
            var filter = new AuditLogFilterDto
            {
                PerformedBy = performedBy,
                EntityName = entityName,
                OperationType = operationType,
                PerformedFrom = performedFrom,
                PerformedTo = performedTo,
                PageNumber = page,
                PageSize = pageSize
            };

            var filterResult = await _auditLogService.GetFilteredAsync(filter);
            if (!filterResult.IsSuccess)
                return View("Error", filterResult.ErrorMessage);

            var paged = filterResult.Data!;

            var totalPages = (int)Math.Ceiling(paged.TotalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var viewModel = new AuditLogListViewModel
            {
                Logs = paged.Items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = paged.TotalCount,
                PerformedBy = performedBy,
                EntityName = entityName,
                OperationType = operationType,
                PerformedFrom = performedFrom,
                PerformedTo = performedTo,
                EntityOptions = await GetEntityOptionsAsync(),
                OperationTypeOptions = GetOperationTypeOptions(),

                PagedResult = new PagedResultViewModel<AuditLog>
                {
                    Items = paged.Items,
                    CurrentPage = page,
                    TotalPages = totalPages
                }
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Details(
       int id,
       string? performedBy,
       string? entityName,
       string? operationType,
       DateTime? performedFrom,
       DateTime? performedTo,
       int page = 1,
       int pageSize = 8)
        {
            var logResult = await _auditLogService.GetByIdAsync(id);
            if (!logResult.IsSuccess)
                return View("Error", logResult.ErrorMessage);

            if (logResult.Data == null) return NotFound();

            var vm = new AuditLogDetailViewModel
            {
                Log = logResult.Data,
                PerformedBy = performedBy,
                EntityName = entityName,
                OperationType = operationType,
                PerformedFrom = performedFrom,
                PerformedTo = performedTo,
                Page = page,
                PageSize = pageSize
            };

            return View("Details", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> Purge()
        {
            var result = await _auditLogService.PurgeAllAsync();
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Error al purgar los logs.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Se purgaron {result.Data} registros de auditoría.";
            return RedirectToAction(nameof(Index));
        }

        private static List<SelectListItem> GetOperationTypeOptions() => new()
    {
        new("Agregado", "Added"),
        new("Modificado", "Modified"),
        new("Eliminado", "Deleted")
    };

        [HttpGet]
        public async Task<IActionResult> Export(
    string? performedBy,
    string? entityName,
    string? operationType,
    DateTime? performedFrom,
    DateTime? performedTo)
        {
            var filter = new AuditLogFilterDto
            {
                PerformedBy = performedBy,
                EntityName = entityName,
                OperationType = operationType,
                PerformedFrom = performedFrom,
                PerformedTo = performedTo,
                PageNumber = 1,
                PageSize = int.MaxValue
            };

            var filterResult = await _auditLogService.GetFilteredAsync(filter);
            if (!filterResult.IsSuccess)
                return RedirectToAction("Index");

            var logs = filterResult.Data!.Items;

            var csvLines = new List<string>
            {
                "Fecha,Usuario,Entidad,Operación,Clave,Valores Anteriores,Valores Nuevos"
            };

            foreach (var log in logs)
            {
                var line = string.Join(",",
                    log.PerformedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    EscapeCsv(log.PerformedBy),
                    EscapeCsv(log.EntityName),
                    EscapeCsv(log.OperationType),
                    EscapeCsv(log.EntityId),
                    EscapeCsv(log.OldValues),
                    EscapeCsv(log.NewValues)
                );

                csvLines.Add(line);
            }

            var csvContent = string.Join(Environment.NewLine, csvLines);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

            return File(bytes, "text/csv", $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            value = value.Replace("\"", "\"\""); // Escapa comillas
            if (value.Contains(",") || value.Contains("\n"))
            {
                value = $"\"{value}\""; // Rodea con comillas si contiene coma o salto de línea
            }
            return value;
        }

        private Task<List<SelectListItem>> GetEntityOptionsAsync()
        {
            var types = ReflectionCacheHelper.GetAuditableEntityTypes();

            var list = types.Select(t => new SelectListItem
            {
                Text = t.Name,
                Value = t.Name
            }).ToList();

            return Task.FromResult(list);
        }

    }
}
