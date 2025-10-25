using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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



        public LibroController(IAuthService authService, ILibroService libroService, IAutorService autorService, IGeneroService generoService, ISeccionService seccionService, IEditorialService editorialService, IImageService imageService)
        {
            _authService = authService;
            _libroService = libroService;
            _autorService = autorService;
            _generoService = generoService;
            _seccionService = seccionService;
            _editorialService = editorialService;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");


                var resultado = await _libroService.BuscarLibrosRapidaAsync("", 1, 20);
                await RecargarViewBagsAsync();


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

        /*
         * Metodo para registar un nuevo libro
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroLibro(CrearLibroFrontDto model)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                // Validaciones básicas
                if (!ModelState.IsValid)
                {
                    await RecargarViewBagsAsync();
                    return View("Index");
                }

                // 1. Validar que se hayan seleccionado autores y géneros
                if (string.IsNullOrEmpty(model.AutoresSeleccionados))
                {
                    ModelState.AddModelError("", "Debe seleccionar al menos un autor");
                    await RecargarViewBagsAsync();
                    return View("Index");
                }

                if (string.IsNullOrEmpty(model.GenerosSeleccionados))
                {
                    ModelState.AddModelError("", "Debe seleccionar al menos un género");
                    await RecargarViewBagsAsync();
                    return View("Index");
                }

                // 2. Procesar la imagen si existe
                string nombrePortada = "/imagenes/portadas/default-book-cover.jpg"; // Valor por defecto

                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    if (_imageService.ValidarImagen(model.ImagenArchivo))
                    {
                        nombrePortada = await _imageService.GuardarPortadaAsync(model.ImagenArchivo, model.ISBN);
                        // Asegurarnos de que tenga el formato de ruta correcto
                        nombrePortada = "/imagenes/portadas/" + nombrePortada;
                    }
                    else
                    {
                        ModelState.AddModelError("ImagenArchivo", "La imagen no es válida. Formatos permitidos: JPG, PNG, GIF. Tamaño máximo: 5MB");
                        await RecargarViewBagsAsync();
                        return View("Index");
                    }
                }

                // 3. Preparar el DTO para la API
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

                // 4. Llamar al servicio para crear el libro
                var resultado = await _libroService.RegistrarLibroAsync(libroDto);

                if (resultado.Success)
                {
                    TempData["SuccessMessage"] = "Libro creado exitosamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", resultado.Message);
                    await RecargarViewBagsAsync();
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                await RecargarViewBagsAsync();
                ViewBag.Error = "Error al crear el libro: " + ex.Message;
                return View("Index");
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

    }
}
