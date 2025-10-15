using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class LibroController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILibroService _libroService;

        public LibroController(IAuthService authService, ILibroService libroService)
        {
            _authService = authService;
            _libroService = libroService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                //var libros = await _libroService.ObtenerLibrosAsync();
                var resultado = await _libroService.BuscarLibrosRapidaAsync("", 1, 20);

                return View(resultado);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de libros";
                //return View(new List<LibroListaView>());
                return View(new PaginacionResponse<LibroListaView>
                {
                    Success = false,
                    Message = "Error al cargar los libros"
                });

            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(string termino, int pagina = 1) {

            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");
                var resultado = await _libroService.BuscarLibrosRapidaAsync(termino, pagina, 20);
                return View("Index",resultado);
            }
            catch (Exception)
            {

                ViewBag.Error = "Error al realizar la busqueda";
                return View("Index", new PaginacionResponse<LibroListaView> { 
                Success = false,
                Message = "Error en la busqueda"

                });

            }

        }
    }
}
