using DemoApp.Web.Models.Interfaces;
using DemoApp.Web.Models.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Web.Models
{
    public class Menu : AuditableEntity, INotAuditableEntity
    {
        [Key]
        public int MenuId { get; set; }

        [DisplayName("Nombre")]
        [StringLength(150)]
        [Required(ErrorMessage = "El Nombre del menú es obligatorio.")]
        public string MenuName { get; set; } = string.Empty;

        [DisplayName("Controlador")]
        [StringLength(50)]
        [Required(ErrorMessage = "El controlador es obligatorio.")]
        public string ControllerName { get; set; } = string.Empty;

        [DisplayName("Entidad")]
        [StringLength(100)]
        public string? EntityName { get; set; }
    }
}
