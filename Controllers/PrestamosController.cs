using System;
using System.Collections.Generic;
using System.Linq;
using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AppWebBiblioteca.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class PrestamosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILibroService _libroService;
        private readonly IAuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PrestamosController(IUsuarioService usuarioService, ILibroService libroService, IAuthService authService, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _usuarioService = usuarioService;
            _libroService = libroService;
            _authService = authService;
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }



        private void AgregarTokenAutenticacion()
        {

            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");

            if (!string.IsNullOrEmpty(token))
            {

                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

        }


        // GET: /Prestamos
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                ViewData["Title"] = "Préstamos y Devoluciones";
                ViewData["PageTitle"] = "Préstamos y Devoluciones";

                // Cargar usuarios activos
                var usuarios = await _usuarioService.ObtenerUsuariosAsync();
                ViewBag.Usuarios = usuarios
                    .Where(u => u.Estado == "Activo")
                    .Select(u => new SelectListItem
                    {
                        Value = u.IdUsuario.ToString(),
                        Text = $"{u.Nombre} - {u.Cedula}"
                    })
                    .ToList();

                // Cargar libros disponibles desde la API
                try
                {
                    AgregarTokenAutenticacion();
                    var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos/libros/disponibles";
                    var response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Deserializar con estructura específica
                        var librosApi = await response.Content.ReadFromJsonAsync<List<LibroDisponibleDto>>();

                        ViewBag.Libros = librosApi?
                            .Select(l => new SelectListItem
                            {
                                Value = l.Id.ToString(),
                                Text = l.DisplayText ?? $"{l.Titulo} - {l.Isbn}"
                            })
                            .ToList() ?? new List<SelectListItem>();
                    }
                    else
                    {
                        // Fallback: usar servicio local si la API falla
                        var libros = await _libroService.ObtenerLibrosAsync();
                        ViewBag.Libros = libros
                            ?.Where(l => l.Estado == "Disponible")
                            .Select(l => new SelectListItem
                            {
                                Value = l.IdLibro.ToString(),
                                Text = $"{l.Titulo} - ISBN: {l.ISBN}"
                            })
                            .ToList() ?? new List<SelectListItem>();
                    }
                }
                catch
                {
                    // Fallback en caso de error de conexión
                    var libros = await _libroService.ObtenerLibrosAsync();
                    ViewBag.Libros = libros
                        ?.Where(l => l.Estado == "Disponible")
                        .Select(l => new SelectListItem
                        {
                            Value = l.IdLibro.ToString(),
                            Text = $"{l.Titulo} - ISBN: {l.ISBN}"
                        })
                        .ToList() ?? new List<SelectListItem>();
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar los datos para préstamos: " + ex.Message;
                ViewBag.Usuarios = new List<SelectListItem>();
                ViewBag.Libros = new List<SelectListItem>();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrestamoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kv => kv.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return Json(new { success = false, message = "Validación fallida", errors });
            }

            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos";
                var prestamoData = new
                {
                    usuarioId = int.Parse(model.UsuarioId),
                    libroId = model.LibroId,
                    fechaPrestamo = model.FechaPrestamo,
                    fechaVencimiento = model.FechaVencimiento,
                    observaciones = model.Observaciones
                };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, prestamoData);

                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Préstamo registrado correctamente." });

                var apiError = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = string.IsNullOrWhiteSpace(apiError) ? "Error al crear el préstamo" : apiError });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPrestamos()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var prestamos = await response.Content.ReadFromJsonAsync<List<object>>();
                    return Json(prestamos);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPrestamosActivos()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos/activos";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var prestamosActivos = await response.Content.ReadFromJsonAsync<List<object>>();
                    return Json(prestamosActivos);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> RegistrarDevolucion(int idPrestamo)
        {
            if (idPrestamo <= 0)
                return BadRequest(new { success = false, message = "ID inválido." });

            try
            {
                AgregarTokenAutenticacion();
                var baseApi = _configuration["ApiSettings:BaseUrl"];
                var url = $"{baseApi}/api/Prestamos/devolucion/{idPrestamo}";

                var response = await _httpClient.PutAsync(url, null);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { success = false, message = body });

                return Json(new { success = true, mensaje = "Devolución registrada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Renovar préstamo 
        [HttpPut]
        public async Task<IActionResult> Renovar(int idPrestamo, DateTime nuevaFechaVencimiento)
        {
            if (idPrestamo <= 0)
                return BadRequest(new { success = false, message = "ID inválido." });
            if (nuevaFechaVencimiento.Date <= DateTime.Today)
                return BadRequest(new { success = false, message = "La nueva fecha debe ser posterior a hoy." });

            try
            {
                AgregarTokenAutenticacion();
                var baseApi = _configuration["ApiSettings:BaseUrl"];
                var url = $"{baseApi}/api/Prestamos/fecha-vencimiento/{idPrestamo}";

                //  Enviar como objeto JSON en el body
                var payload = new
                {
                    fechaVencimiento = nuevaFechaVencimiento
                };

                var response = await _httpClient.PutAsJsonAsync(url, payload);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                        var errorMessage = errorResponse?.ContainsKey("message") == true
                            ? errorResponse["message"].ToString()
                            : body;
                        return StatusCode((int)response.StatusCode, new { success = false, message = errorMessage });
                    }
                    catch
                    {
                        return StatusCode((int)response.StatusCode, new { success = false, message = body });
                    }
                }

                // Parsear respuesta exitosa
                try
                {
                    var resultado = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                    return Json(new
                    {
                        success = true,
                        mensaje = "Préstamo renovado correctamente",
                        data = resultado
                    });
                }
                catch
                {
                    return Json(new { success = true, mensaje = "Préstamo renovado correctamente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error de conexión: " + ex.Message });
            }
        }

        //Buscar préstamos
        [HttpGet]
        public async Task<IActionResult> Buscar(string? termino)
        {
            // Validar que el término de búsqueda no esté vacío
            if (string.IsNullOrWhiteSpace(termino))
            {
                return BadRequest(new { success = false, message = "Debe indicar al menos un criterio de búsqueda." });
            }

            try
            {
                AgregarTokenAutenticacion();
                var baseApi = _configuration["ApiSettings:BaseUrl"];
                // Construir URL directamente con el parámetro 'termino' que espera la API
                var url = $"{baseApi}/api/Prestamos/buscar?termino={Uri.EscapeDataString(termino)}";

                var response = await _httpClient.GetAsync(url);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { success = false, message = raw });

                // La API devuelve directamente la lista de préstamos
                var lista = await response.Content.ReadFromJsonAsync<List<object>>() ?? new List<object>();
                return Json(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        //Obtener libros disponibles desde la API
        [HttpGet]
        public async Task<IActionResult> GetLibrosDisponibles()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos/libros/disponibles";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        message = "Error al cargar libros disponibles",
                        error = errorContent
                    });
                }

                var libros = await response.Content.ReadFromJsonAsync<List<LibroDisponibleDto>>();
                return Json(libros ?? new List<LibroDisponibleDto>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al cargar libros disponibles",
                    error = ex.Message
                });
            }
        }
    }
}





