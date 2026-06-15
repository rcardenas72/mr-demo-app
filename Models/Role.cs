using System.ComponentModel.DataAnnotations;
using DemoApp.Web.Models.Shared;

namespace DemoApp.Web.Models
{
    public class Role : AuditableEntity
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(50)]
        [Display(Name = "Nombre")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        // Propiedad de navegación hacia Users
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();

        // Propiedad de navegación hacia Permissions
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
