using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

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

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                var usuarios = await _usuarioService.ObtenerUsuariosAsync();
                await CargarRolesAsync();
                return View(usuarios);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de usuarios";
                await CargarRolesAsync();
                return View(new List<UsuarioListaViewModel>());
            }
        }

        private async Task CargarRolesAsync(int? rolSeleccionadoId = null)
        {
            var roles = await _rolService.ObtenerRolesAsync();

            // Agregar opción por defecto
            var rolesConDefault = new List<RolDto>
    {
        new RolDto { Id = 0, Nombre = "Seleccione un rol..." }
    };
            rolesConDefault.AddRange(roles);

            ViewBag.Roles = new SelectList(rolesConDefault, "Id", "Nombre", rolSeleccionadoId);
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

            await CargarRolesAsync();
            return View(new RegistroUsuarioDto());
        }


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




        /*
         * 
         */
        // En UsuarioController.cs

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AsignarRol(AsignacionRolDto dto)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            TempData["ErrorMessage"] = "Debe iniciar sesión para realizar esta acción";
        //            return RedirectToAction("Login");
        //        }

        //        if (dto.IdUsuario <= 0 || dto.IdRol <= 0)
        //        {
        //            TempData["ErrorMessage"] = "Datos inválidos para asignar rol";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        var (ok, mensaje) = await _rolService.AsignarRolAUsuarioAsync(dto);

        //        if (ok)
        //        {
        //            TempData["SuccessMessage"] = mensaje ?? "Rol asignado correctamente";
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = mensaje ?? "Error al asignar el rol";
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error interno al asignar rol";
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(AsignacionRolDto dto)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    TempData["ErrorMessage"] = "Debe iniciar sesión para realizar esta acción";
                    return RedirectToAction("Login");
                }

                if (dto.IdUsuario <= 0 || dto.IdRol <= 0)
                {
                    TempData["ErrorMessage"] = "Datos inválidos para asignar rol";
                    return RedirectToAction(nameof(Index));
                }

                // 👇 BLOQUEO: no permitir asignar a usuarios Inactivos
                var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(dto.IdUsuario);
                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "El usuario no existe";
                    return RedirectToAction(nameof(Index));
                }

                var estado = (usuario.Estado ?? string.Empty).Trim();
                if (estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "No se pueden añadir roles al usuario porque está inactivo.";
                    return RedirectToAction(nameof(Index));
                }
                // ☝️ Fin bloqueo

                var (ok, mensaje) = await _rolService.AsignarRolAUsuarioAsync(dto);

                TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                    mensaje ?? (ok ? "Rol asignado correctamente" : "Error al asignar el rol");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error interno al asignar rol";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarRol(AsignacionRolDto dto)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    TempData["ErrorMessage"] = "Debe iniciar sesión para realizar esta acción";
                    return RedirectToAction("Login");
                }

                if (dto.IdUsuario <= 0 || dto.IdRol <= 0)
                {
                    TempData["ErrorMessage"] = "Datos inválidos para quitar rol";
                    return RedirectToAction(nameof(Index));
                }

                // Necesitarías un método en tu servicio para quitar roles
                var (ok, mensaje) = await _rolService.QuitarRolAUsuarioAsync(dto);

                if (ok)
                {
                    TempData["SuccessMessage"] = mensaje ?? "Rol quitado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = mensaje ?? "Error al quitar el rol";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error interno al quitar rol";
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
