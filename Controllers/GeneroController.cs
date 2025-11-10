using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class GeneroController : Controller
    {
        private readonly IGeneroService _generoService;
        private readonly IAuthService _authService;

        public GeneroController(IGeneroService generoService, IAuthService authService)
        {
            _generoService = generoService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                // Si tu servicio aún no tiene la búsqueda paginada, reemplaza por ObtenerGenerosAsync(nombre) y arma tú la vista.
                PaginacionResponse<GeneroDto> resultado =
                    await _generoService.BuscarGenerosRapidaAsync(termino ?? string.Empty, pagina, resultadosPorPagina);

                ViewBag.TerminoBusqueda = termino ?? string.Empty;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                return View(resultado);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de géneros";
                return View(new PaginacionResponse<GeneroDto>
                {
                    Success = false,
                    Message = "Error al cargar los géneros",
                    Data = new List<GeneroDto>(),
                    Pagination = new PaginationInfo
                    {
                        PaginaActual = pagina,
                        ResultadosPorPagina = resultadosPorPagina,
                        TotalResultados = 0,
                        TotalPaginas = 0
                    }
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string nombre, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "El nombre es requerido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var id = await _generoService.RegistrarGeneroAsync(nombre.Trim());
                TempData[id > 0 ? "Ok" : "Error"] = id > 0
                    ? "Género creado con éxito."
                    : "No se pudo crear el género.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo crear el género. Detalle: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idGenero, string nombre, string? returnUrl = null)
        {
            if (idGenero <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "Datos de edición inválidos.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ok = await _generoService.EditarGeneroAsync(idGenero, nombre.Trim());
                TempData[ok ? "Ok" : "Error"] = ok
                    ? "Género actualizado correctamente."
                    : "No se pudo actualizar el género.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar género: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idGenero, string? returnUrl = null)
        {
            if (idGenero <= 0)
            {
                TempData["Error"] = "Id inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ok = await _generoService.EliminarGeneroAsync(idGenero);
                TempData[ok ? "Ok" : "Error"] = ok
                    ? "Género eliminado correctamente."
                    : "No se pudo eliminar el género.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar género: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Tab(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        {
            try
            {
                var resultado = await _generoService.BuscarGenerosRapidaAsync(
                    termino ?? string.Empty, pagina, resultadosPorPagina);

                ViewBag.TerminoBusqueda = termino ?? string.Empty;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                // Devuelve partial si viene desde AJAX (X-Requested-With)
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_TabGeneros", resultado);

                // Si no, renderiza la vista normal (por ejemplo si se abre directo)
                return View("Index", resultado);
            }
            catch
            {
                return PartialView("_TabGeneros", new PaginacionResponse<GeneroDto>
                {
                    Success = false,
                    Message = "Error al cargar los géneros",
                    Data = new List<GeneroDto>(),
                    Pagination = new PaginationInfo
                    {
                        PaginaActual = pagina,
                        ResultadosPorPagina = resultadosPorPagina,
                        TotalResultados = 0,
                        TotalPaginas = 0
                    }
                });
            }
        }

    }
}
