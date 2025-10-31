using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

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

        //[Authorize(Policy = "AdminOnly")]
        //[HttpGet]
        //public async Task<IActionResult> Index()
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //            return RedirectToAction("Login", "Usuario");

        //        var usuarios = await _usuarioService.ObtenerUsuariosAsync();
        //        await CargarRolesAsync();
        //        return View(usuarios);
        //    }
        //    catch
        //    {
        //        ViewBag.Error = "Error al cargar la lista de usuarios";
        //        await CargarRolesAsync();
        //        return View(new List<UsuarioListaViewModel>());
        //    }
        //}

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Index(string termino = "", int pagina = 1, int resultadosPorPagina = 20)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                PaginacionResponse<UsuarioListaViewModel> resultado;

                if (!string.IsNullOrEmpty(termino))
                {
                    // Buscar usuarios por término (nombre o cédula)
                    resultado = await _usuarioService.BuscarUsuariosRapidaAsync(termino, pagina, resultadosPorPagina);
                }
                else
                {
                    // Listar todos los usuarios paginados
                    resultado = await _usuarioService.BuscarUsuariosRapidaAsync("", pagina, resultadosPorPagina);
                }

                await CargarRolesAsync();

                ViewBag.TerminoBusqueda = termino;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                return View(resultado);
            }
            catch
            {
                ViewBag.Error = "Error al cargar la lista de usuarios";
                await CargarRolesAsync();

                return View(new PaginacionResponse<UsuarioListaViewModel>
                {
                    Success = false,
                    Message = "Error al cargar los usuarios"
                });
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
            


                    //TempData["SuccessMessage"] = $"¡Bienvenido {resultado.Nombre}!";
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
        public async Task<IActionResult> CrearUsuario(RegistroUsuarioDto usuario, int? idRol)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });

                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Datos del usuario inválidos" });

                var apiResponse = await _usuarioService.CrearUsuarioAsync(usuario);

                if (!apiResponse.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = apiResponse.Message,
                        detail = apiResponse.Data
                    });
                }

                // Asignación de rol (igual que antes)
                if (idRol.HasValue && idRol.Value > 0)
                {
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
                            apiResponse.Message += $" {msgRol ?? "Rol asignado correctamente."}";
                        else
                            apiResponse.Message += $" {msgRol ?? "Usuario creado, pero no se pudo asignar el rol."}";
                    }
                    else
                    {
                        apiResponse.Message += " No se pudo localizar su Id para asignar el rol.";
                    }
                }

                return Json(new
                {
                    success = true,
                    message = apiResponse.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error interno del sistema al crear usuario",
                    detail = ex.Message
                });
            }
        }















        //[HttpGet]
        //public async Task<IActionResult> Crear()
        //{
        //    if (!_authService.IsAuthenticated())
        //        return RedirectToAction("Login", "Usuario");

        //    await CargarRolesAsync();
        //    return View(new RegistroUsuarioDto());
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CrearUsuario(RegistroUsuarioDto usuario, int? idRol) // <-- idRol separado
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
        //            await CargarRolesAsync(); // sin preselección
        //            return View("Crear", usuario);
        //        }

        //        var creado = await _usuarioService.CrearUsuarioAsync(usuario);
        //        if (!creado)
        //        {
        //            TempData["ErrorMessage"] = "Error al crear el usuario";
        //            await CargarRolesAsync();
        //            return View("Crear", usuario);
        //        }

        //        // Si viene un rol seleccionado, asignarlo usando el DTO nuevo
        //        if (idRol.HasValue && idRol.Value > 0)
        //        {
        //            // localizar el IdUsuario recién creado (asume email único)
        //            var usuarios = await _usuarioService.ObtenerUsuariosAsync();
        //            var creadoVm = usuarios.FirstOrDefault(u =>
        //                string.Equals(u.Email?.Trim(), usuario.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

        //            if (creadoVm != null)
        //            {
        //                var dto = new AsignacionRolDto
        //                {
        //                    IdUsuario = creadoVm.IdUsuario,
        //                    IdRol = idRol.Value
        //                };

        //                var (okRol, msgRol) = await _rolService.AsignarRolAUsuarioAsync(dto);
        //                if (okRol)
        //                    TempData["SuccessMessage"] = msgRol ?? "Usuario creado y rol asignado correctamente.";
        //                else
        //                    TempData["ErrorMessage"] = msgRol ?? "Usuario creado, pero no se pudo asignar el rol.";
        //            }
        //            else
        //            {
        //                TempData["SuccessMessage"] = "Usuario creado. No se pudo localizar su Id para asignar el rol.";
        //            }
        //        }
        //        else
        //        {
        //            TempData["SuccessMessage"] = "Usuario creado exitosamente.";
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        TempData["ErrorMessage"] = "Error interno del sistema al crear usuario";
        //        await CargarRolesAsync();
        //        return View("Crear", usuario);
        //    }
        //}


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
        //            await CargarRolesAsync();
        //            return View("Editar", usuario);
        //        }

        //        var actualizado = await _usuarioService.ActualizarUsuarioAsync(usuario);

        //        if (actualizado)
        //        {
        //            TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        TempData["ErrorMessage"] = "Error al actualizar el usuario";
        //        await CargarRolesAsync();
        //        return View("Editar", usuario);
        //    }
        //    catch
        //    {
        //        TempData["ErrorMessage"] = "Error interno del sistema al actualizar usuario";
        //        await CargarRolesAsync();
        //        return View("Editar", usuario);
        //    }
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditarUsuario(EditarUsuarioDto usuario)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción", type = "error" });
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            var errors = ModelState.Values
        //                .SelectMany(v => v.Errors)
        //                .Select(e => e.ErrorMessage)
        //                .ToList();
        //            return Json(new { success = false, message = "Datos del usuario inválidos: " + string.Join(", ", errors), type = "error" });
        //        }

        //        var actualizado = await _usuarioService.ActualizarUsuarioAsync(usuario);

        //        if (actualizado)
        //        {
        //            return Json(new { success = true, message = "✅ Usuario actualizado exitosamente", type = "success" });
        //        }

        //        return Json(new { success = false, message = "❌ Error al actualizar el usuario", type = "error" });
        //    }
        //    catch (InvalidOperationException ex) when (ex.Message.Contains("ya existe") || ex.Message.Contains("duplicado"))
        //    {
        //        // Captura errores específicos de duplicados
        //        return Json(new { success = false, message = $"❌ {ex.Message}", type = "error" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"❌ Error interno del sistema: {ex.Message}", type = "error" });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioDto usuario)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    // Si es AJAX → devolver JSON
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = "Sesión no válida. Inicie sesión nuevamente." });

                    return RedirectToAction("Login", "Usuario");
                }

                if (!ModelState.IsValid)
                {
                    var mensaje = "Datos del usuario inválidos. Por favor, verifique la información.";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = mensaje });

                    ViewData["ModalError"] = mensaje;
                    await CargarRolesAsync();
                    return View("Editar", usuario);
                }

                var apiResponse = await _usuarioService.ActualizarUsuarioAsync(usuario);

                // ✅ Si la solicitud viene de AJAX → devolver JSON directamente
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = apiResponse.Success,
                        message = apiResponse.Message,
                        detail = apiResponse.Data
                    });
                }

                // ✅ Si es un submit normal → usar ViewData y mostrar modal
                if (apiResponse.Success)
                {
                    ViewData["ModalSuccess"] = apiResponse.Message;
                }
                else
                {
                    ViewData["ModalError"] = apiResponse.Message;
                    if (apiResponse.Data != null)
                        ViewData["ModalDetail"] = apiResponse.Data.ToString();
                }

                await CargarRolesAsync();
                return View("Editar", usuario);
            }
            catch (Exception ex)
            {
                var mensajeError = "Error interno del sistema al actualizar usuario";

                // AJAX → JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = mensajeError, detail = ex.Message });

                // No AJAX → View
                ViewData["ModalError"] = mensajeError;
                ViewData["ModalDetail"] = ex.Message;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol([FromBody] AsignacionRolDto dto)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
                }

                if (dto.IdUsuario <= 0 || dto.IdRol <= 0)
                {
                    return Json(new { success = false, message = "Datos inválidos para asignar rol" });
                }

                var (ok, mensaje) = await _rolService.AsignarRolAUsuarioAsync(dto);

                return Json(new
                {
                    success = ok,
                    message = mensaje ?? (ok ? "Rol asignado correctamente" : "Error al asignar el rol")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno al asignar rol" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarRol([FromBody] AsignacionRolDto dto)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
                }

                if (dto.IdUsuario <= 0 || dto.IdRol <= 0)
                {
                    return Json(new { success = false, message = "Datos inválidos para quitar rol" });
                }

                var (ok, mensaje) = await _rolService.QuitarRolAUsuarioAsync(dto);

                return Json(new
                {
                    success = ok,
                    message = mensaje ?? (ok ? "Rol quitado correctamente" : "Error al quitar el rol")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno al quitar rol" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PerfilUsuario()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                // Obtener el ID del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Llamar al servicio - ahora puede lanzar excepciones
                var perfil = await _usuarioService.PerfilUsuarioViewAsync(userId);

                if (perfil == null)
                {
                    TempData["Error"] = "No se encontró información del perfil";
                    return View(new PerfilUsuarioDto());
                }

                return View(perfil);
            }
            catch (HttpRequestException ex)
            {
                // Manejar errores específicos de la API
                TempData["Error"] = $"Error de conexión: {ex.Message}";
                return View(new PerfilUsuarioDto());
            }
            catch (Exception ex)
            {
                // Manejar otros errores
                TempData["Error"] = "Error interno al cargar el perfil";
                // Log: _logger.LogError(ex, "Error al cargar perfil para usuario {UserId}", userId);
                return View(new PerfilUsuarioDto());
            }
        }






        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditarPerfil(PerfilUsuarioDto perfilUsuario)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //            return RedirectToAction("Login", "Usuario");

        //        if (!ModelState.IsValid)
        //        {
        //            TempData["ErrorMessage"] = "Datos del usuario inválidos";
        //            return View("PerfilUsuario", perfilUsuario);
        //        }

        //        var actualizado = await _usuarioService.ActualizarPerfilsync(perfilUsuario);

        //        if (actualizado)
        //        {
        //            TempData["SuccessMessage"] = "Perfil actualizado exitosamente";
        //            return RedirectToAction(nameof(PerfilUsuario));
        //        }

        //        TempData["ErrorMessage"] = "Error al actualizar el perfil";

        //        return View("PerfilUsuario", perfilUsuario);
        //    }
        //    catch
        //    {
        //        TempData["ErrorMessage"] = "Error interno del sistema al actualizar el perfil";
        //        return View("PerfilUsuario", perfilUsuario);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(PerfilUsuarioDto perfilUsuario, bool DebeReautenticar)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Datos del usuario inválidos";
                    return View("PerfilUsuario", perfilUsuario);
                }

                var actualizado = await _usuarioService.ActualizarPerfilsync(perfilUsuario);

                if (actualizado)
                {
                    if (DebeReautenticar)
                    {
                        //TempData["SuccessMessage"] = "Tu perfil fue actualizado. Por seguridad, inicia sesión nuevamente.";
                        // Cierra sesión y redirige al login
                        await _authService.LogoutAsync();
                        return RedirectToAction("Login", "Usuario");
                    }

                    TempData["SuccessMessage"] = "Perfil actualizado exitosamente";
                    return RedirectToAction(nameof(PerfilUsuario));
                }

                TempData["ErrorMessage"] = "Error al actualizar el perfil";
                return View("PerfilUsuario", perfilUsuario);
            }
            catch
            {
                TempData["ErrorMessage"] = "Error interno del sistema al actualizar el perfil";
                return View("PerfilUsuario", perfilUsuario);
            }
        }



        // ======== LOGOUT ========

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            //TempData["SuccessMessage"] = "Sesión cerrada exitosamente";

            
            return RedirectToAction("Login");
        }
    }
}
