using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class AutorController : Controller
    {
        private readonly IAutorService _autorService;
        private readonly IAuthService _authService;

        public AutorController(IAutorService autorService, IAuthService authService)
        {
            _autorService = autorService;
            _authService = authService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                PaginacionResponse<AutorDto> resultado;

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    // Buscar autores por término (nombre)
                    resultado = await _autorService.BuscarAutoresRapidaAsync(termino, pagina, resultadosPorPagina);
                }
                else
                {
                    // Listar todos los autores paginados
                    resultado = await _autorService.BuscarAutoresRapidaAsync(string.Empty, pagina, resultadosPorPagina);
                }

                ViewBag.TerminoBusqueda = termino;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                return View(resultado); // Vista tipada a PaginacionResponse<AutorDto>
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de autores";
                return View(new PaginacionResponse<AutorDto>
                {
                    Success = false,
                    Message = "Error al cargar los autores",
                    Data = new List<AutorDto>(),
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


        // POST: /Autor/Crear
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
                var id = await _autorService.RegistrarAutorAsync(nombre.Trim());

                // Si no hubo excepción, asumimos creación exitosa aunque el id sea 0 (fallback)
                if (id > 0)
                {
                    TempData["Ok"] = "Autor creado con éxito.";
                    TempData["AutorCreadoId"] = id;
                }
                else
                {
                    // Fallback cuando la API no devuelve id pero sí creó el autor
                    TempData["Ok"] = "Autor creado con éxito.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo crear el autor. Detalle: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }


        // POST: /Autor/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idAutor, string nombre, string? returnUrl = null)
        {
            if (idAutor <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "Datos de edición inválidos.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ok = await _autorService.EditarAutorAsync(idAutor, nombre.Trim());
                TempData[ok ? "Ok" : "Error"] = ok
                    ? "Autor actualizado correctamente."
                    : "No se pudo actualizar el autor.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar autor: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Autor/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idAutor, string? returnUrl = null)
        {
            if (idAutor <= 0)
            {
                TempData["Error"] = "Id inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ok = await _autorService.EliminarAutorAsync(idAutor);
                TempData[ok ? "Ok" : "Error"] = ok
                    ? "Autor eliminado correctamente."
                    : "No se pudo eliminar el autor.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar autor: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
