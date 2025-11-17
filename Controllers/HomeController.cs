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

        [HttpPost]
        public async Task<IActionResult> FiltrarEstadisticas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var filtro = new FiltroEstadisticasDTO
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                var estadisticas = await _estadisticaService.ObtenerEstadisticasPorFiltroAsync(filtro);
                return PartialView("_EstadisticasPartial", estadisticas);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "Error al filtrar estadísticas");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DescargarReporte(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var filtro = new FiltroEstadisticasDTO
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                var excelBytes = await _estadisticaService.DescargarReporteExcelAsync(filtro);

                var fileName = $"ReportePrestamos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                
                TempData["Error"] = "Error al generar el reporte Excel";
                return RedirectToAction("Index");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
