using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class GestionBibliograficaController : Controller
    {

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
