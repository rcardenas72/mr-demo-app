using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace DemoApp.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;

        public AccountController(IUserService userService, IPermissionService permissionService)
        {
            _userService = userService;
            _permissionService = permissionService;
        }


        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError("", "El nombre de usuario es obligatorio.");
                return View();
            }

            var userResult = await _userService.GetProfileAsync(username);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View();
            }

            if (!userResult.Data.IsActive)
            {
                ModelState.AddModelError("", "Usuario inactivo.");
                return View();
            }

            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("UserName", username);
            HttpContext.Session.SetString("IsAdmin", userResult.Data.IsAdmin.ToString().ToLower());
            HttpContext.Session.SetString("RoleName", userResult.Data.RoleName ?? "");

            var permissionsResult = await _permissionService.GetAccessibleMenuOptionsAsync(username);
            var permissions = permissionsResult.IsSuccess ? permissionsResult.Data! : new List<string>();
            HttpContext.Session.SetString("UserPermissions", JsonSerializer.Serialize(permissions));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

#pragma warning disable CS0162
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme);
#pragma warning restore CS0162
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                TempData["AccessDeniedMessage"] = "No se pudo obtener la información del usuario.";
                return RedirectToAction("AccessDenied", "Error");
            }

            var username = email.Split('@')[0];
            var userResult = await _userService.GetProfileAsync(username);

            if (!userResult.IsSuccess)
            {
                TempData["AccessDeniedMessage"] = "Error al obtener perfil.";
                return RedirectToAction("AccessDenied", "Error");
            }

            if (userResult.Data == null)
            {
                TempData["AccessDeniedMessage"] = "Usuario no encontrado.";
                return RedirectToAction("AccessDenied", "Error");
            }

            return View(userResult.Data);
        }

    }

}
