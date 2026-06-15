using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DemoApp.Web.Models.Shared;

namespace DemoApp.Web.Models
{
    public class AppUser : AuditableEntity
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;

        [StringLength(60)]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(60)]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        [EmailAddress]
        public string Email { get; set; }= string.Empty;

        [Required]
        public int RoleId { get; set; }

        // Propiedad de navegación a Rol
        public Role Role { get; set; } = null!;

        [NotMapped]
        [Display(Name = "Rol")]
        public string RoleName => Role?.RoleName ?? string.Empty;

        [Display(Name = "¿Es Administrador?")]
        public bool IsAdmin { get; set; }

        [DisplayName("Activo")]
        public bool IsActive { get; set; } = true;

        public string FullName => $"{FirstName} {LastName}";
    }
}
