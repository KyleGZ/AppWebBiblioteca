using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class AutorController : Controller
    {
        private readonly IAutorService _autorService;
        private readonly IAuthService _authService;
        private readonly IBitacoraService _bitacoraService;

        //constructor
        public AutorController(IAutorService autorService, IAuthService authService, IBitacoraService bitacoraService)
        {
            _autorService = autorService;
            _authService = authService;
            _bitacoraService = bitacoraService;
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

                return View(resultado); 
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

        // ================== CRUD JSON PARA TABS ==================
        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string nombre, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Json(new { success = false, message = "El nombre es requerido." });

            try
            {
                var resultado = await _autorService.RegistrarAutorAsync(nombre.Trim());

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    var idAutor = await _autorService.ObtenerIdAutor(nombre);
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "CREAR_AUTOR", "AUTOR", idAutor);
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Autor creado con éxito.",
                        data = resultado.Data
                    });
                }

                return Json(new
                {
                    success = false,
                    message = resultado.Message ?? "No se pudo crear el autor."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"No se pudo crear el autor. Detalle: {ex.Message}" });
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idAutor, string nombre, string? returnUrl = null)
        {
            if (idAutor <= 0 || string.IsNullOrWhiteSpace(nombre))
                return Json(new { success = false, message = "Datos de edición inválidos." });

            try
            {
                var resultado = await _autorService.EditarAutorAsync(idAutor, nombre.Trim());

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "EDITAR_AUTOR", "AUTOR", idAutor);
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Autor actualizado correctamente.",
                        data = resultado.Data
                    });
                }

                return Json(new
                {
                    success = false,
                    message = resultado.Message ?? "No se pudo actualizar el autor."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar autor: {ex.Message}" });
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idAutor, string? returnUrl = null)
        {
            if (idAutor <= 0)
                return Json(new { success = false, message = "Id inválido." });

            try
            {
                var resultado = await _autorService.EliminarAutorAsync(idAutor);

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "ELIMINAR_AUTOR", "AUTOR", idAutor);
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Autor eliminado correctamente.",
                        data = resultado.Data
                    });
                }

                return Json(new
                {
                    success = false,
                    message = resultado.Message ?? "No se pudo eliminar el autor."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar autor: {ex.Message}" });
            }
        }

        //Tab
        [HttpGet]
        public async Task<IActionResult> Tab(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        {
            if (!_authService.IsAuthenticated())
                return Unauthorized();

            var resultado = await _autorService.BuscarAutoresRapidaAsync(
                termino ?? string.Empty, pagina, resultadosPorPagina);

            ViewBag.TerminoBusqueda = termino;
            return PartialView("_TabAutores", resultado);
        }

    }
}
