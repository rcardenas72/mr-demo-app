using DemoApp.Web.Helpers;
using DemoApp.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Web.ViewModels
{
    public class PermissionFormViewModel
    {
        public int? PermissionId { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "El menú es obligatorio.")]
        [Display(Name = "Menú")]
        public int MenuId { get; set; }

        [Display(Name = "Tipo de Operación")]
        [Required(ErrorMessage = "La operación es obligatoria.")]
        public OperationType OperationType { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> Roles { get; set; } = new();
        public List<SelectListItem> Menus { get; set; } = new();
        public List<SelectListItem> Operations { get; set; } = new();

        public string? RoleName { get; set; }
        public string? MenuName { get; set; }
        public string? OperationName => OperationTypeHelper.GetOperationName(OperationType);

    }
}
