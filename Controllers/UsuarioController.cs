using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppWebBiblioteca.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;

        public UsuarioController(
            IAuthService authService,
            IUsuarioService usuarioService,
            IRolService rolService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                var usuarios = await _usuarioService.ObtenerUsuariosAsync();
                return View(usuarios);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de usuarios";
                return View(new List<UsuarioListaViewModel>());
            }
        }

        private async Task CargarRolesAsync(int? rolSeleccionadoId = null)
        {
            var roles = await _rolService.ObtenerRolesAsync(); // [{Id, Nombre}]
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre", rolSeleccionadoId);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_authService.IsAuthenticated())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var resultado = await _authService.LoginAsync(model);

                if (resultado.Resultado)
                {
                    TempData["SuccessMessage"] = $"¡Bienvenido {resultado.Nombre}!";
                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = resultado.Msj ?? "Credenciales inválidas. Por favor, inténtalo de nuevo.";
                return View(model);
            }
            catch
            {
                TempData["ErrorMessage"] = "Error de conexión con el servidor. Por favor, inténtalo más tarde.";
                return View(model);
            }
        }

        // ======== CREAR ========

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Usuario");

            await CargarRolesAsync(); // Dropdown de roles
            return View(new RegistroUsuarioDto());
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CrearUsuario(RegistroUsuarioDto usuario)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            TempData["ErrorMessage"] = "Debe iniciar sesión para realizar esta acción";
        //            return RedirectToAction("Login");
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            TempData["ErrorMessage"] = "Datos del usuario inválidos";
        //            await CargarRolesAsync(usuario.IdRol); // mantener selección
        //            return View("Crear", usuario);
        //        }

        //        var resultado = await _usuarioService.CrearUsuarioAsync(usuario);

        //        if (resultado)
        //        {
        //            TempData["SuccessMessage"] = "Usuario creado exitosamente";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        TempData["ErrorMessage"] = "Error al crear el usuario";
        //        await CargarRolesAsync(usuario.IdRol);
        //        return View("Crear", usuario);
        //    }
        //    catch
        //    {
        //        TempData["ErrorMessage"] = "Error interno del sistema al crear usuario";
        //        await CargarRolesAsync(usuario.IdRol);
        //        return View("Crear", usuario);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(RegistroUsuarioDto usuario, int? idRol) // <-- idRol separado
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
                    await CargarRolesAsync(); // sin preselección
                    return View("Crear", usuario);
                }

                var creado = await _usuarioService.CrearUsuarioAsync(usuario);
                if (!creado)
                {
                    TempData["ErrorMessage"] = "Error al crear el usuario";
                    await CargarRolesAsync();
                    return View("Crear", usuario);
                }

                // Si viene un rol seleccionado, asignarlo usando el DTO nuevo
                if (idRol.HasValue && idRol.Value > 0)
                {
                    // localizar el IdUsuario recién creado (asume email único)
                    var usuarios = await _usuarioService.ObtenerUsuariosAsync();
                    var creadoVm = usuarios.FirstOrDefault(u =>
                        string.Equals(u.Email?.Trim(), usuario.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (creadoVm != null)
                    {
                        var dto = new AsignacionRolDto
                        {
                            IdUsuario = creadoVm.IdUsuario,
                            IdRol = idRol.Value
                        };

                        var (okRol, msgRol) = await _rolService.AsignarRolAUsuarioAsync(dto);
                        if (okRol)
                            TempData["SuccessMessage"] = msgRol ?? "Usuario creado y rol asignado correctamente.";
                        else
                            TempData["ErrorMessage"] = msgRol ?? "Usuario creado, pero no se pudo asignar el rol.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Usuario creado. No se pudo localizar su Id para asignar el rol.";
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "Usuario creado exitosamente.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error interno del sistema al crear usuario";
                await CargarRolesAsync();
                return View("Crear", usuario);
            }
        }




        // ======== EDITAR ========

        //[HttpGet]
        //public async Task<IActionResult> Editar(int id)
        //{
        //    if (!_authService.IsAuthenticated())
        //        return RedirectToAction("Login", "Usuario");

        //    var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
        //    if (usuario == null) return NotFound();

        //    var dto = new EditarUsuarioDto
        //    {
        //        IdUsuario = usuario.IdUsuario,
        //        Nombre = usuario.Nombre,
        //        Email = usuario.Email,
        //        IdRol = usuario.IdRol,
        //        // mapear otros campos si aplica...
        //    };

        //    await CargarRolesAsync(dto.IdRol);
        //    return View(dto);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditarUsuario(EditarUsuarioDto usuario)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //            return RedirectToAction("Login", "Usuario");

        //        if (!ModelState.IsValid)
        //        {
        //            TempData["ErrorMessage"] = "Datos del usuario inválidos";
        //            await CargarRolesAsync(usuario.IdRol);
        //            return View("Editar", usuario);
        //        }

        //        var resultado = await _usuarioService.ActualizarUsuarioAsync(usuario);

        //        if (resultado)
        //        {
        //            TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        TempData["ErrorMessage"] = "Error al actualizar el usuario";
        //        await CargarRolesAsync(usuario.IdRol);
        //        return View("Editar", usuario);
        //    }
        //    catch
        //    {
        //        TempData["ErrorMessage"] = "Error interno del sistema al actualizar usuario";
        //        await CargarRolesAsync(usuario.IdRol);
        //        return View("Editar", usuario);
        //    }
        //}

        // ======== EDITAR ========
        // Ya no mapeamos rol único porque existe tabla puente UsuarioRol.
        // La edición de datos no toca roles; roles se gestionan con acciones separadas.

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Usuario");

            var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
            if (usuario == null) return NotFound();

            var dto = new EditarUsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Email = usuario.Email
                // (sin IdRol)
            };

            await CargarRolesAsync(); // lista de roles para UI (asignar/quitar aparte)
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioDto usuario)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Datos del usuario inválidos";
                    await CargarRolesAsync();
                    return View("Editar", usuario);
                }

                var actualizado = await _usuarioService.ActualizarUsuarioAsync(usuario);

                if (actualizado)
                {
                    TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Error al actualizar el usuario";
                await CargarRolesAsync();
                return View("Editar", usuario);
            }
            catch
            {
                TempData["ErrorMessage"] = "Error interno del sistema al actualizar usuario";
                await CargarRolesAsync();
                return View("Editar", usuario);
            }
        }


        // ======== ESTADO ========

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            try
            {
                var resultado = await _usuarioService.DesactivarUsuarioAsync(id);

                TempData[resultado ? "SuccessMessage" : "ErrorMessage"] =
                    resultado ? "Usuario desactivado exitosamente" : "Error al desactivar el usuario";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error interno del sistema al desactivar usuario";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            try
            {
                var resultado = await _usuarioService.ActivarUsuarioAsync(id);

                TempData[resultado ? "SuccessMessage" : "ErrorMessage"] =
                    resultado ? "Usuario activado exitosamente" : "Error al activar el usuario";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error interno del sistema al activar usuario";
                return RedirectToAction(nameof(Index));
            }
        }

        // ======== LOGOUT ========

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["SuccessMessage"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login");
        }
    }
}
