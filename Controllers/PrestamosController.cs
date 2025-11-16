using System;
using System.Collections.Generic;
using System.Linq;
using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace AppWebBiblioteca.Controllers
{
    [Authorize]
    public class PrestamosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILibroService _libroService;
        private readonly IAuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PrestamosController(IUsuarioService usuarioService, ILibroService libroService, IAuthService authService, HttpClient httpClient, IConfiguration configuration)
        {
            _usuarioService = usuarioService;
            _libroService = libroService;
            _authService = authService;
            _httpClient = httpClient;
            _configuration = configuration;
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

                // Obtener usuarios reales desde la API
                var usuarios = await _usuarioService.ObtenerUsuariosAsync();
                ViewBag.Usuarios = usuarios
                    .Where(u => u.Estado == "Activo")
                    .Select(u => new SelectListItem 
                    { 
                        Value = u.IdUsuario.ToString(), 
                        Text = $"{u.Nombre} - {u.Cedula}" 
                    })
                    .ToList();

                // Obtener libros reales desde la API
                var libros = await _libroService.ObtenerLibrosAsync();
                ViewBag.Libros = libros
                    ?.Where(l => l.Estado == "Disponible")
                    .Select(l => new SelectListItem 
                    { 
                        Value = l.IdLibro.ToString(), 
                        Text = $"{l.Titulo} - ISBN: {l.ISBN}"
                    })
                    .ToList() ?? new List<SelectListItem>();

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
                {
                    return Json(new { success = true, message = "Préstamo registrado correctamente." });
                }
                
                return Json(new { success = false, message = "Error al crear el préstamo" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // MÉTODOS ACTUALIZADOS PARA USAR LA API REAL
        [HttpGet]
        public async Task<IActionResult> GetPrestamos()
        {
            try
            {
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
        public async Task<IActionResult> RegistrarDevolucion(long id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Prestamos/devolucion/{id}";
                var response = await _httpClient.PutAsync(apiUrl, null);
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, mensaje = "Devolución registrada exitosamente" });
                }
                
                return BadRequest(new { success = false, message = "Error al registrar la devolución" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}


