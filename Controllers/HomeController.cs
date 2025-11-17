using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AppWebBiblioteca.Controllers
{
  
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEstadisticaService _estadisticaService;

        public HomeController(IAuthService authService, IEstadisticaService estadisticaService)
        {
            _authService = authService;
            _estadisticaService = estadisticaService;
        }

        public async Task<IActionResult> Index()
        {

            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Usuario");

            var estadisticas = await _estadisticaService.ObtenerEstadisticasAsync();

            ViewBag.Estadisticas = estadisticas;
            //ViewBag.UserEmail = User.Identity?.Name ?? "Usuario";
            //ViewBag.UserRoles = new List<string> { "Bibliotecario" }; // Ajusta según tu sistema de roles
            ViewBag.UserEmail = _authService.GetUserEmail();
           
            ViewBag.UserRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
