using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AppWebBiblioteca.Controllers
{
    public class SeccionController : Controller
    {
        private readonly ISeccionService _seccionService;
        private readonly IAuthService _authService;

        public SeccionController(ISeccionService seccionService, IAuthService authService)
        {
            _seccionService = seccionService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                // Si tu servicio aún no tiene la búsqueda paginada, reemplaza por ObtenerSeccionesAsync(nombre).
                PaginacionResponse<SeccionDto> resultado =
                    await _seccionService.BuscarSeccionesRapidaAsync(termino ?? string.Empty, pagina, resultadosPorPagina);

                ViewBag.TerminoBusqueda = termino ?? string.Empty;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                return View(resultado);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de secciones";
                return View(new PaginacionResponse<SeccionDto>
                {
                    Success = false,
                    Message = "Error al cargar las secciones",
                    Data = new List<SeccionDto>(),
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

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string nombre, string? ubicacion, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return Json(new { success = false, message = "El nombre es requerido." });
            }

            try
            {
                var resultado = await _seccionService.RegistrarSeccionAsync(nombre.Trim(), ubicacion?.Trim());

                if (resultado.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Sección creada con éxito.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo crear la sección."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"No se pudo crear la sección. Detalle: {ex.Message}"
                });
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Crear(string nombre, string? ubicacion, string? returnUrl = null)
        //{
        //    if (string.IsNullOrWhiteSpace(nombre))
        //    {
        //        TempData["Error"] = "El nombre es requerido.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    try
        //    {
        //        var id = await _seccionService.RegistrarSeccionAsync(nombre.Trim(), ubicacion?.Trim());
        //        TempData[id > 0 ? "Ok" : "Error"] = id > 0
        //            ? "Sección creada con éxito."
        //            : "No se pudo crear la sección.";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"No se pudo crear la sección. Detalle: {ex.Message}";
        //    }

        //    if (!string.IsNullOrEmpty(returnUrl))
        //        return LocalRedirect(returnUrl);

        //    return RedirectToAction(nameof(Index));
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Editar(int idSeccion, string nombre, string? ubicacion, string? returnUrl = null)
        //{
        //    if (idSeccion <= 0 || string.IsNullOrWhiteSpace(nombre))
        //    {
        //        TempData["Error"] = "Datos de edición inválidos.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    try
        //    {
        //        var ok = await _seccionService.EditarSeccionAsync(idSeccion, nombre.Trim(), ubicacion?.Trim());
        //        TempData[ok ? "Ok" : "Error"] = ok
        //            ? "Sección actualizada correctamente."
        //            : "No se pudo actualizar la sección.";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"Error al actualizar sección: {ex.Message}";
        //    }

        //    if (!string.IsNullOrEmpty(returnUrl))
        //        return LocalRedirect(returnUrl);

        //    return RedirectToAction(nameof(Index));
        //}



        /*
         * Eliminar Sección
         */

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Eliminar(int idSeccion, string? returnUrl = null)
        //{
        //    if (idSeccion <= 0)
        //    {
        //        TempData["Error"] = "Id inválido.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    try
        //    {
        //        var ok = await _seccionService.EliminarSeccionAsync(idSeccion);
        //        TempData[ok ? "Ok" : "Error"] = ok
        //            ? "Sección eliminada correctamente."
        //            : "No se pudo eliminar la sección.";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"Error al eliminar sección: {ex.Message}";
        //    }

        //    if (!string.IsNullOrEmpty(returnUrl))
        //        return LocalRedirect(returnUrl);

        //    return RedirectToAction(nameof(Index));
        //}


        /*
         * Metodo para editar sección que devuelve JSON
         */
        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idSeccion, string nombre, string? ubicacion, string? returnUrl = null)
        {
            if (idSeccion <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                return Json(new { success = false, message = "Datos de edición inválidos." });
            }

            try
            {
                var resultado = await _seccionService.EditarSeccionAsync(idSeccion, nombre.Trim(), ubicacion?.Trim());

                if (resultado.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Sección actualizada correctamente.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo actualizar la sección."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al actualizar sección: {ex.Message}"
                });
            }
        }

        /*
         * Metodo para eliminar sección que devuelve JSON
         */
        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idSeccion, string? returnUrl = null)
        {
            if (idSeccion <= 0)
            {
                return Json(new { success = false, message = "Id inválido." });
            }

            try
            {
                var resultado = await _seccionService.EliminarSeccionAsync(idSeccion);

                if (resultado.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Sección eliminada correctamente.",
                        data = resultado.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo eliminar la sección."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al eliminar sección: {ex.Message}"
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Tab(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        {
            try
            {
                var resultado = await _seccionService.BuscarSeccionesRapidaAsync(
                    termino ?? string.Empty, pagina, resultadosPorPagina);

                ViewBag.TerminoBusqueda = termino ?? string.Empty;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_TabSecciones", resultado);

                return View("Index", resultado);
            }
            catch
            {
                return PartialView("_TabSecciones", new PaginacionResponse<SeccionDto>
                {
                    Success = false,
                    Message = "Error al cargar las secciones",
                    Data = new List<SeccionDto>(),
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
