using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class AutorController : Controller
    {
        private readonly IAutorService _autorService;

        public AutorController(IAutorService autorService)
        {
            _autorService = autorService;
        }

        // GET: /Autor?nombre=Gab
        public async Task<IActionResult> Index(string? nombre)
        {
            try
            {
                var autores = await _autorService.ObtenerAutoresAsync(nombre);
                ViewBag.Filtro = nombre;
                return View(autores); // Vista fuertemente tipada a List<AutorDto>
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo cargar la lista de autores: {ex.Message}";
                return View(new List<AutorDto>());
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

        // (Opcional) GET: /Autor/BuscarJson?term=Gar
        // Útil para autocompletar/Select2 en creación de libros
        [HttpGet]
        public async Task<IActionResult> BuscarJson(string? term)
        {
            var autores = await _autorService.ObtenerAutoresAsync(term);
            return Json(autores);
        }
    }
}
