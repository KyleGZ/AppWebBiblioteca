using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    [Authorize] // Opcional: restringe a usuarios autenticados
    public class PrestamosController : Controller
    {
        // GET: /Prestamos
        public IActionResult Index()
        {
            ViewData["Title"] = "Préstamos y Devoluciones";
            ViewData["PageTitle"] = "Préstamos y Devoluciones";
            return View();
        }
    }
}

