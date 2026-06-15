using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemoApp.Web.ViewModels
{
    public class UserFormViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no debe superar los 50 caracteres")]
        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(60, ErrorMessage = "Los nombres no deben superar los 60 caracteres")]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(60, ErrorMessage = "Los apellidos no deben superar los 60 caracteres")]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [StringLength(50, ErrorMessage = "El correo electrónico no debe superar los 50 caracteres")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Rol")]
        public int RoleId { get; set; }

        [Display(Name = "Administrador")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; }

        // Para el formulario (DropDownList)
        public List<SelectListItem>? Roles { get; set; }
    }
}
