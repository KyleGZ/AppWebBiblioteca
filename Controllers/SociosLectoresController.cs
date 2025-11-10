using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class SociosLectoresController : Controller
    {

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
