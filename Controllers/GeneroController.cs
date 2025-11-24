using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class GeneroController : Controller
    {
        private readonly IGeneroService _generoService;
        private readonly IAuthService _authService;
        private readonly IBitacoraService _bitacoraService;

        public GeneroController(IGeneroService generoService, IAuthService authService, IBitacoraService bitacoraService)
        {
            _generoService = generoService;
            _authService = authService;
            _bitacoraService = bitacoraService;
            
        }

        [Authorize(Policy = ("AuthenticatedUsers"))]
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

        //Metodo para crear
        [Authorize(Policy = ("StaffOnly"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string nombre, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return Json(new { success = false, message = "El nombre es requerido." });
            }

            try
            {
                
                var resultado = await _generoService.RegistrarGeneroAsync(nombre.Trim());

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    var idGenero =  await _generoService.ObtenerIdGenero(nombre);
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "CREAR_GENERO", "GENERO", idGenero);
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Género creado con éxito.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo crear el género."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"No se pudo crear el género. Detalle: {ex.Message}"
                });
            }
        }

        //Metodo para editar
        [Authorize(Policy = ("StaffOnly"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idGenero, string nombre, string? returnUrl = null)
        {
            if (idGenero <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                return Json(new { success = false, message = "Datos de edición inválidos." });
            }

            try
            {
                var resultado = await _generoService.EditarGeneroAsync(idGenero, nombre.Trim());

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                   
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "EDITAR_GENERO", "GENERO", idGenero);

                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Género actualizado correctamente.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo actualizar el género."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al actualizar género: {ex.Message}"
                });
            }
        }

        //Metodo para eliminar
        [Authorize(Policy = ("StaffOnly"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idGenero, string? returnUrl = null)
        {
            if (idGenero <= 0)
            {
                return Json(new { success = false, message = "Id inválido." });
            }

            try
            {
                var resultado = await _generoService.EliminarGeneroAsync(idGenero);

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "ELIMINAR_GENERO", "GENERO", idGenero);
                    return Json(new
                    {

                        success = true,
                        message = resultado.Message ?? "Género eliminado correctamente.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo eliminar el género."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al eliminar género: {ex.Message}"
                });
            }
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
