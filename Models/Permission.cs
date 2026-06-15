using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DemoApp.Web.Helpers;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Models.Shared;

namespace DemoApp.Web.Models
{
    public class Permission : AuditableEntity
    {
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "El Rol es obligatorio.")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "El menú es obligatorio.")]
        public int MenuId { get; set; }

        [DisplayName("Tipo de operación")]
        [Required(ErrorMessage = "El tipo de operación es obligatorio.")]
        public OperationType OperationType { get; set; } 

        [DisplayName("Activo")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        [DisplayName("Menú")]
        public string MenuName { get; set; } = string.Empty;

        [NotMapped]
        [DisplayName("Rol")]
        public string RoleName { get; set; } = string.Empty;

        [NotMapped]
        public string OperationName => OperationTypeHelper.GetOperationName(OperationType);

        public Role Role { get; set; } = null!;

        public Menu Menu { get; set; } = null!;

    }
}
