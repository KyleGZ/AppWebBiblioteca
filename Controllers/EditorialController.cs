using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class EditorialController : Controller
    {
        private readonly IEditorialService _editorialService;
        private readonly IAuthService _authService;
        private readonly IBitacoraService _bitacoraService;

        public EditorialController(IEditorialService editorialService, IAuthService authService, IBitacoraService bitacoraService)
        {
            _editorialService = editorialService;
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

                PaginacionResponse<EditorialDto> resultado =
                    await _editorialService.BuscarEditorialesRapidaAsync(termino ?? string.Empty, pagina, resultadosPorPagina);

                ViewBag.TerminoBusqueda = termino ?? string.Empty;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                return View(resultado);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de editoriales";
                return View(new PaginacionResponse<EditorialDto>
                {
                    Success = false,
                    Message = "Error al cargar las editoriales",
                    Data = new List<EditorialDto>(),
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

        // POST: /Editorial/Crear
        [Authorize(Policy = "StaffOnly")]
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
                var resultado = await _editorialService.RegistrarEditorialAsync(nombre.Trim());

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    var idEditorial = await _editorialService.ObtenerIdEditorial(nombre);
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "CREAR_EDITORIAL", "EDITORIAL", idEditorial);
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Editorial creada con éxito.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo crear la editorial."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"No se pudo crear la editorial. Detalle: {ex.Message}"
                });
            }
        }

        // POST: /Editorial/Editar
        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idEditorial, string nombre, string? returnUrl = null)
        {
            if (idEditorial <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                return Json(new { success = false, message = "Datos de edición inválidos." });
            }

            try
            {
                var resultado = await _editorialService.EditarEditorialAsync(idEditorial, nombre.Trim());

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "EDITAR_EDITORIAL", "EDITORIAL", idEditorial);
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Editorial actualizada correctamente.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo actualizar la editorial."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al actualizar editorial: {ex.Message}"
                });
            }
        }

        // POST: /Editorial/Eliminar
        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idEditorial, string? returnUrl = null)
        {
            if (idEditorial <= 0)
            {
                return Json(new { success = false, message = "Id inválido." });
            }

            try
            {
                var resultado = await _editorialService.EliminarEditorialAsync(idEditorial);

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "ELIMINAR_EDITORIAL", "EDITORIAL", idEditorial);
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Editorial eliminada correctamente.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo eliminar la editorial (puede tener libros asociados)."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al eliminar editorial: {ex.Message}"
                });
            }
        }

        // GET: /Editorial/Tab?termino=&pagina=1&resultadosPorPagina=20
        [HttpGet]
        public async Task<IActionResult> Tab(string termino = "", int pagina = 1, int resultadosPorPagina = 20)
        {
            if (!_authService.IsAuthenticated())
                return Unauthorized("No autenticado");

            var modelo = await _editorialService.BuscarEditorialesRapidaAsync(termino ?? "", pagina, resultadosPorPagina);

            // Debe existir: Views/Editorial/_TabEditoriales.cshtml
            return PartialView("_TabEditoriales", modelo);
        }

    }
}
