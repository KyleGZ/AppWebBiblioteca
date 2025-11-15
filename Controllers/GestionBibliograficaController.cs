using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class GestionBibliograficaController : Controller
    {
        private readonly IAutorService _autorService;
        private readonly IAuthService _authService;

        public GestionBibliograficaController(IAuthService authorizacion)
        {
            _authService = authorizacion;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Usuario");
            return View();
        }
    }
}
