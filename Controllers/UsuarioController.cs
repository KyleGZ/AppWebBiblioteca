using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public UsuarioController(IAuthService authService, IConfiguration configuration, IUsuarioService usuarioService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _usuarioService.ObtenerUsuariosAsync();
                return View(usuarios);
            }
            catch (Exception ex)
            {
                // Manejar error - puedes pasar un mensaje a la vista
                ViewBag.Error = "Error al cargar la lista de usuarios";
                return View(new List<UsuarioListaViewModel>());
            }
        }




        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al home
            if (_authService.IsAuthenticated())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, mostrar la vista con errores
                return View(model);
            }

            try
            {
                var resultado = await _authService.LoginAsync(model);

                if (resultado.Resultado)
                {
                    // Login exitoso
                    TempData["SuccessMessage"] = $"¡Bienvenido {resultado.Nombre}!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Login fallido
                    TempData["ErrorMessage"] = resultado.Msj ?? "Credenciales inválidas. Por favor, inténtalo de nuevo.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // Error de conexión con la API
                TempData["ErrorMessage"] = "Error de conexión con el servidor. Por favor, inténtalo más tarde.";
                return View(model);
            }
        }

        /*
         * 
         */
        [HttpPost]
        public async Task<IActionResult> CrearUsuario(RegistroUsuarioDto usuario)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    TempData["ErrorMessage"] = "Debe iniciar sesión para realizar esta acción";
                    return RedirectToAction("Login");
                }

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Datos del usuario inválidos";
                    return RedirectToAction("Index");
                }

                var resultado = await _usuarioService.CrearUsuarioAsync(usuario);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Usuario creado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al crear el usuario";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error interno del sistema al crear usuario";
                return RedirectToAction("Index");
            }
        }

        /*
         * 
         */
        [HttpPost]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioDto usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Datos del usuario inválidos";
                    return RedirectToAction("Index");
                }

                var resultado = await _usuarioService.ActualizarUsuarioAsync(usuario);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al actualizar el usuario";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error interno del sistema al actualizar usuario";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            try
            {
                var resultado = await _usuarioService.DesactivarUsuarioAsync(id);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Usuario desactivado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al desactivar el usuario";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error interno del sistema al desactivar usuario";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            try
            {
                var resultado = await _usuarioService.ActivarUsuarioAsync(id);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Usuario activado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al activar el usuario";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error interno del sistema al activar usuario";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["SuccessMessage"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login");
        }


    }
}
