using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace AppWebBiblioteca.Controllers
{
    public class LibroController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILibroService _libroService;
        private readonly IAutorService _autorService;
        private readonly IGeneroService _generoService;
        private readonly ISeccionService _seccionService;
        private readonly IEditorialService _editorialService;
        private readonly IImageService _imageService;
        private readonly IBitacoraService _bitacoraService;


        public LibroController(IAuthService authService, ILibroService libroService, IAutorService autorService, IGeneroService generoService, ISeccionService seccionService, IEditorialService editorialService, IImageService imageService, IBitacoraService bitacoraService)
        {
            _authService = authService;
            _libroService = libroService;
            _autorService = autorService;
            _generoService = generoService;
            _seccionService = seccionService;
            _editorialService = editorialService;
            _imageService = imageService;
            _bitacoraService = bitacoraService;
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet] 
        public async Task<IActionResult> Index(string termino = "", bool buscarPorDescripcion = false, int pagina = 1, int resultadosPorPagina = 20)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                PaginacionResponse<LibroListaView> resultado;

                if (buscarPorDescripcion && !string.IsNullOrEmpty(termino))
                {
                    resultado = await _libroService.BuscarLibrosDescripcionAsync(termino, pagina, resultadosPorPagina);
                }
                else if (!string.IsNullOrEmpty(termino))
                {
                    resultado = await _libroService.BuscarLibrosRapidaAsync(termino, pagina, resultadosPorPagina);
                }
                else
                {
                    resultado = await _libroService.BuscarLibrosRapidaAsync("", pagina, resultadosPorPagina);
                }

                await RecargarViewBagsAsync();

                ViewBag.TerminoBusqueda = termino;
                ViewBag.BuscarPorDescripcion = buscarPorDescripcion;

                return View(resultado);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de libros";
                await RecargarViewBagsAsync();

                return View(new PaginacionResponse<LibroListaView>
                {
                    Success = false,
                    Message = "Error al cargar los libros"
                });
            }
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpPost]
        public async Task<IActionResult> Buscar(string termino, bool buscarPorDescripcion = false, int pagina = 1)
        {

            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                if (buscarPorDescripcion)
                {
                    var resultado = await _libroService.BuscarLibrosDescripcionAsync(termino, pagina, 20);
                    return View("Index", resultado);

                }
                else
                {
                    var resultado = await _libroService.BuscarLibrosRapidaAsync(termino, pagina, 20);
                    return View("Index", resultado);
                }

            }
            catch (Exception)
            {

                ViewBag.Error = "Error al realizar la busqueda";
                return View("Index", new PaginacionResponse<LibroListaView>
                {
                    Success = false,
                    Message = "Error en la busqueda"

                });

            }

        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroLibro(CrearLibroFrontDto model)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });

                var idUsuario = _authService.GetUserId();
                // Validaciones básicas
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "El modelo del libro no es válido." });
                }

                // Validar que se hayan seleccionado autores y géneros
                if (string.IsNullOrEmpty(model.AutoresSeleccionados))
                {
                    return Json(new { success = false, message = "Debe seleccionar al menos un autor" });
                }

                if (string.IsNullOrEmpty(model.GenerosSeleccionados))
                {
                    return Json(new { success = false, message = "Debe seleccionar al menos un género" });
                }

                // Procesar la imagen si existe
                string nombrePortada = "/imagenes/portadas/default-book-cover.jpg";

                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    try
                    {
                        if (!_imageService.ValidarImagen(model.ImagenArchivo))
                        {
                            return Json(new { success = false, message = "La imagen no es válida. Formatos permitidos: JPG, PNG, GIF. Tamaño máximo: 5MB" });
                        }

                        // Procesar imagen
                        nombrePortada = await _imageService.GuardarPortadaAsync(model.ImagenArchivo, model.ISBN);
                        nombrePortada = "/imagenes/portadas/" + nombrePortada;
                    }
                    catch (Exception exImagen)
                    {
                        return Json(new { success = false, message = $"Error al procesar la imagen: {exImagen.Message}" });
                    }
                }

                // Preparar el DTO para la API
                var libroDto = new CrearLibroDto
                {
                    Titulo = model.Titulo,
                    Isbn = model.ISBN,
                    IdEditorial = model.EditorialId,
                    IdSeccion = model.SeccionId,
                    Estado = model.Estado,
                    Descripcion = model.Descripcion ?? "",
                    PortadaUrl = nombrePortada,
                    IdAutores = model.AutoresSeleccionados.Split(',').Select(int.Parse).ToList(),
                    IdGeneros = model.GenerosSeleccionados.Split(',').Select(int.Parse).ToList()
                };

                // Llamar al servicio para crear el libro
                var resultado = await _libroService.RegistrarLibroAsync(libroDto);

                if (resultado.Success)
                {

                    var libroId = await _libroService.ObtenerIdLibro(model.ISBN);
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value,"CREAR_LIBRO", "LIBRO", libroId);

                    return Json(new
                    {
                        success = true,
                        message = "Libro creado exitosamente",
                        data = new { libroId = resultado.Data } 

                        
                    });

                }
                else
                {
                    return Json(new { success = false, message = resultado.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear el libro: " + ex.Message });
            }
        }



        /*
         * Metodo para cargar datos del libro a editar
         */
        [Authorize(Policy = "StaffOnly")]
        [HttpGet]
        public async Task<IActionResult> ObtenerLibroParaEditar(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "No autenticado" });

                var libro = await _libroService.ObtenerLibroParaEditarAsync(id);
                if (libro == null)
                {
                    return Json(new { success = false, message = "No se encontró el libro solicitado." });
                }

                return Json(new { success = true, data = libro });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar el libro para editar." });
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int idLibro, CrearLibroFrontDto model)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });

                // Validaciones básicas
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "El modelo del libro no es válido." });

                // Validar autores y géneros
                if (string.IsNullOrEmpty(model.AutoresSeleccionados))
                    return Json(new { success = false, message = "Debe seleccionar al menos un autor" });

                if (string.IsNullOrEmpty(model.GenerosSeleccionados))
                    return Json(new { success = false, message = "Debe seleccionar al menos un género" });

                // Procesar la imagen si se subió una nueva
                string nombrePortada = null;
                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    try
                    {
                        if (!_imageService.ValidarImagen(model.ImagenArchivo))
                        {
                            return Json(new
                            {
                                success = false,
                                message = "La imagen no es válida. Formatos permitidos: JPG, PNG, GIF. Tamaño máximo: 5MB"
                            });
                        }

                        nombrePortada = await _imageService.GuardarPortadaAsync(model.ImagenArchivo, model.ISBN);
                        nombrePortada = "/imagenes/portadas/" + nombrePortada;
                    }
                    catch (Exception exImagen)
                    {
                        return Json(new { success = false, message = $"Error al procesar la imagen: {exImagen.Message}" });
                    }
                }

                // Preparar el DTO para la API
                var libroDto = new CrearLibroDto
                {
                    Titulo = model.Titulo,
                    Isbn = model.ISBN,
                    IdEditorial = model.EditorialId,
                    IdSeccion = model.SeccionId,
                    Estado = model.Estado,
                    Descripcion = model.Descripcion ?? "",
                    PortadaUrl = nombrePortada, 
                    IdAutores = model.AutoresSeleccionados.Split(',').Select(int.Parse).ToList(),
                    IdGeneros = model.GenerosSeleccionados.Split(',').Select(int.Parse).ToList()
                };

                // Llamar al servicio de edición
                var resultado = await _libroService.EditarLibroAsync(idLibro, libroDto);

                if (resultado.Success)
                {
                    var idUsuario = _authService.GetUserId();
                    await _bitacoraService.RegistrarAccionAsync(idUsuario.Value, "EDITAR_LIBRO", "LIBRO", idLibro);
                    return Json(new
                    {
                        success = true,
                        message = "Libro actualizado exitosamente",
                        data = new { libroId = idLibro }
                    });
                }
                else
                {
                    return Json(new { success = false, message = resultado.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el libro: " + ex.Message });
            }
        }


        /*
         * Metodo para ver el detalle de un libro
         */
        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleLibro(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "No autenticado" });

                var libro = await _libroService.ObtenerDetalleLibroAsync(id);
                if (libro == null)
                {
                    return Json(new { success = false, message = "No se encontró el libro solicitado." });
                }

                return Json(new { success = true, data = libro });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar el detalle del libro." });
            }
        }





        /*
         * Metodos privados para cargar datos en los SelectList
         */

        private async Task RecargarViewBagsAsync()
        {
            await CargarAutoresAsync();
            await CargarGenerosAsync();
            await CargarSeccionesAsync();
            await CargarEditorialesAsync();
        }

        private async Task CargarAutoresAsync(string? nombre = "")
        {

            var autores = await _autorService.ObtenerAutoresAsync(nombre);

            var autorDefecto = new List<AutorDto>
            {
                new AutorDto { IdAutor = 0, Nombre = "-- Seleccione un autor --" }
            };

            autorDefecto.AddRange(autores);

            ViewBag.Autores = new SelectList(autorDefecto, "IdAutor", "Nombre");
        }

        private async Task CargarGenerosAsync(string? nombre = "")
        {
            var generos = await _generoService.ObtenerGenerosAsync(nombre);
            var generoDefecto = new List<GeneroDto>
            {
                new GeneroDto { IdGenero = 0, Nombre = "-- Seleccione un género --" }
            };
            generoDefecto.AddRange(generos);
            ViewBag.Generos = new SelectList(generoDefecto, "IdGenero", "Nombre");
        }

        private async Task CargarSeccionesAsync(string? nombre = "")
        {
            var secciones = await _seccionService.ObtenerSeccionesAsync(nombre);
            var seccionDefecto = new List<SeccionDto>
            {
                new SeccionDto { IdSeccion = 0, Nombre = "-- Seleccione una sección --" }
            };
            seccionDefecto.AddRange(secciones);
            ViewBag.Secciones = new SelectList(seccionDefecto, "IdSeccion", "Nombre");
        }

        private async Task CargarEditorialesAsync(string? nombre = "")
        {
            var editoriales = await _editorialService.ObtenerEditorialesAsync(nombre);
            var editorialDefecto = new List<EditorialDto>
            {
                new EditorialDto { IdEditorial = 0, Nombre = "-- Seleccione una editorial --" }
            };
            editorialDefecto.AddRange(editoriales);
            ViewBag.Editoriales = new SelectList(editorialDefecto, "IdEditorial", "Nombre");
        }


        /*
         * 
         */
        [Authorize(Policy = "StaffOnly")]
        [HttpGet]
        public async Task<IActionResult> DescargarPlantillaImportacion()
        {
            try
            {
                var contenido = await _libroService.DescargarPlantillaImportacionAsync();

                return File(
                    contenido,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Plantilla_Importacion_Libros.xlsx"
                );
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al descargar plantilla: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        public async Task<IActionResult> ImportarLibrosExcel(IFormFile archivo)
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Usuario");

            var esAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // Validaciones
            if (archivo == null || archivo.Length == 0)
                return ResponderConMensaje("Debe adjuntar un archivo Excel válido.", false, esAjax);

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!new[] { ".xlsx", ".xls" }.Contains(extension))
                return ResponderConMensaje("Solo se permiten archivos Excel (.xlsx, .xls).", false, esAjax);

            if (archivo.Length > 5 * 1024 * 1024) // 5MB
                return ResponderConMensaje("El archivo no puede ser mayor a 5MB.", false, esAjax);

            try
            {
                var resultado = await _libroService.ImportarLibrosDesdeExcelAsync(archivo);

                if (esAjax)
                {
                    return resultado.Success
                        ? Ok(new { success = true, message = resultado.Message, datos = SanitizarDatos(resultado.Data) })
                        : BadRequest(new { success = false, message = resultado.Message, datos = SanitizarDatos(resultado.Data) });
                }

                // Para requests normales
                TempData[resultado.Success ? "Success" : "Error"] = resultado.Message;

                if (resultado.Success)
                    TempData["ImportacionData"] = JsonSerializer.Serialize(resultado.Data);
                else
                    TempData["ImportacionErrores"] = JsonSerializer.Serialize(resultado.Data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var mensaje = $"Ocurrió un error inesperado: {ex.Message}";
                return ResponderConMensaje(mensaje, false, esAjax);
            }
        }

        private object SanitizarDatos(object datos)
        {
            // Sanitizar datos para no exponer información sensible en respuestas AJAX
            if (datos is JsonElement element)
            {
                try
                {
                    var jsonString = element.GetRawText();
                    var document = JsonDocument.Parse(jsonString);
                    var root = document.RootElement;

                    // Remover propiedades sensibles
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        using var stream = new MemoryStream();
                        using var writer = new Utf8JsonWriter(stream);
                        writer.WriteStartObject();

                        foreach (var prop in root.EnumerateObject())
                        {
                            if (!prop.Name.ToLowerInvariant().Contains("stacktrace") &&
                                !prop.Name.ToLowerInvariant().Contains("innerexception"))
                            {
                                prop.WriteTo(writer);
                            }
                        }

                        writer.WriteEndObject();
                        writer.Flush();

                        return JsonSerializer.Deserialize<object>(stream.ToArray());
                    }
                }
                catch
                {
                    // Si falla la sanitización, retornar null
                    return null;
                }
            }

            return datos;
        }

        private IActionResult ResponderConMensaje(string mensaje, bool esExito, bool esAjax)
        {
            if (esAjax)
            {
                return esExito
                    ? Ok(new { success = true, message = mensaje })
                    : BadRequest(new { success = false, message = mensaje });
            }

            TempData[esExito ? "Success" : "Error"] = mensaje;
            return RedirectToAction("Index");
        }
    }
}
