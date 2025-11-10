using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class EditorialController : Controller
    {
        private readonly IEditorialService _editorialService;
        private readonly IAuthService _authService;

        public EditorialController(IEditorialService editorialService, IAuthService authService)
        {
            _editorialService = editorialService;
            _authService = authService;
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
                var id = await _editorialService.RegistrarEditorialAsync(nombre.Trim());

                if (id > 0)
                {
                    TempData["Ok"] = "Editorial creada con éxito.";
                    TempData["EditorialCreadaId"] = id;
                }
                else
                {
                    TempData["Ok"] = "Editorial creada con éxito.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo crear la editorial. Detalle: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Editorial/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idEditorial, string nombre, string? returnUrl = null)
        {
            if (idEditorial <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "Datos de edición inválidos.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ok = await _editorialService.EditarEditorialAsync(idEditorial, nombre.Trim());
                TempData[ok ? "Ok" : "Error"] = ok
                    ? "Editorial actualizada correctamente."
                    : "No se pudo actualizar la editorial.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar editorial: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Editorial/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idEditorial, string? returnUrl = null)
        {
            if (idEditorial <= 0)
            {
                TempData["Error"] = "Id inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ok = await _editorialService.EliminarEditorialAsync(idEditorial);
                TempData[ok ? "Ok" : "Error"] = ok
                    ? "Editorial eliminada correctamente."
                    : "No se pudo eliminar la editorial (puede tener libros asociados).";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar editorial: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
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
