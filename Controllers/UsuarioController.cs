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
        private readonly ILogger<UsuarioController> _logger;
        private readonly IBitacoraService _bitacoraService;

        public UsuarioController(
            IAuthService authService,
            IUsuarioService usuarioService,
            IRolService rolService,
            ILogger<UsuarioController> logger,
            IBitacoraService bitacoraService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _rolService = rolService;
            _logger = logger;
            _bitacoraService = bitacoraService;
        }


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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CrearUsuario(RegistroUsuarioDto usuario, int? idRol)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //            return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });


        //        if (!ModelState.IsValid)
        //            return Json(new { success = false, message = "Datos del usuario inválidos" });

        //        var apiResponse = await _usuarioService.CrearUsuarioAsync(usuario);

        //        if (!apiResponse.Success)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = apiResponse.Message,
        //                detail = apiResponse.Data
        //            });
        //        }

        //        // Obtener información de la sesión actual para bitácora
        //        var userSession = _authService.GetUserId();

        //        int idUsuarioCreado = 0;
        //        var usuarios = await _usuarioService.ObtenerUsuariosAsync();
        //        var creadoVm = usuarios.FirstOrDefault(u =>
        //            string.Equals(u.Email?.Trim(), usuario.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

        //        if (creadoVm != null)
        //        {
        //            idUsuarioCreado = creadoVm.IdUsuario;

        //            // Registrar en bitácora
        //            _ = _bitacoraService.RegistrarAccionAsync(userSession, "Creación", "usuario", idUsuarioCreado);

        //        }


        //        // Asignación de rol (igual que antes)
        //        if (idRol.HasValue && idRol.Value > 0)
        //        {


        //            if (creadoVm != null)
        //            {
        //                var dto = new AsignacionRolDto
        //                {
        //                    IdUsuario = creadoVm.IdUsuario,
        //                    IdRol = idRol.Value
        //                };

        //                var (okRol, msgRol) = await _rolService.AsignarRolAUsuarioAsync(dto);
        //                if (okRol)
        //                    apiResponse.Message += $" {msgRol ?? "Rol asignado correctamente."}";
        //                else
        //                    apiResponse.Message += $" {msgRol ?? "Usuario creado, pero no se pudo asignar el rol."}";
        //            }
        //            else
        //            {
        //                apiResponse.Message += " No se pudo localizar su Id para asignar el rol.";
        //            }
        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            message = apiResponse.Message
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Error interno del sistema al crear usuario",
        //            detail = ex.Message
        //        });
        //    }
        //}

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

                // Obtener información de la sesión actual para bitácora
                var userSession = _authService.GetUserId();

                if(userSession > 0)
                {
                    // Si no se pudo obtener el ID de usuario, registrar error en bitácora
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_OBTENER_ID_USUARIO", "Usuarios", 0);
                }


                int idUsuarioCreado = 0;
                var usuarios = await _usuarioService.ObtenerUsuariosAsync();
                var creadoVm = usuarios.FirstOrDefault(u =>
                    string.Equals(u.Email?.Trim(), usuario.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

                if (creadoVm != null)
                {
                    idUsuarioCreado = creadoVm.IdUsuario;

                    if(userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "CREAR_USUARIO", "Usuarios", idUsuarioCreado);
                }

                // Asignación de rol (igual que antes)
                if (idRol.HasValue && idRol.Value > 0)
                {
                    if (creadoVm != null)
                    {
                        var dto = new AsignacionRolDto
                        {
                            IdUsuario = creadoVm.IdUsuario,
                            IdRol = idRol.Value
                        };

                        var (okRol, msgRol) = await _rolService.AsignarRolAUsuarioAsync(dto);
                        if (okRol)
                        {
                            apiResponse.Message += $" {msgRol ?? "Rol asignado correctamente."}";

                            if (userSession.HasValue)
                                _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ASIGNAR_ROL", "UsuarioRoles", idUsuarioCreado);
                        }
                        else
                        {
                            apiResponse.Message += $" {msgRol ?? "Usuario creado, pero no se pudo asignar el rol."}";

                            if (userSession.HasValue)
                                _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_ASIGNAR_ROL", "UsuarioRoles", idUsuarioCreado);
                        }
                    }
                    else
                    {
                        apiResponse.Message += " No se pudo localizar su Id para asignar el rol.";

                        if (userSession.HasValue)
                            _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "USUARIO_NO_ENCONTRADO", "Usuarios", 0);
                    }
                }
                else
                {
                    if(userSession.HasValue)
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "USUARIO_CREADO_SIN_ROL", "Usuarios", idUsuarioCreado);
                }

                return Json(new
                {
                    success = true,
                    message = apiResponse.Message
                });
            }
            catch (Exception ex)
            {
                // REGISTRAR EN BITÁCORA - ERROR EN CREACIÓN
                var userSession = _authService.GetUserId();
                if(userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_CREAR_USUARIO", "Usuarios", 0);

                return Json(new
                {
                    success = false,
                    message = "Error interno del sistema al crear usuario",
                    detail = ex.Message
                });
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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditarUsuario(EditarUsuarioDto usuario)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            // Si es AJAX → devolver JSON
        //            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //                return Json(new { success = false, message = "Sesión no válida. Inicie sesión nuevamente." });

        //            return RedirectToAction("Login", "Usuario");
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            var mensaje = "Datos del usuario inválidos. Por favor, verifique la información.";

        //            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //                return Json(new { success = false, message = mensaje });

        //            ViewData["ModalError"] = mensaje;
        //            await CargarRolesAsync();
        //            return View("Editar", usuario);
        //        }

        //        var apiResponse = await _usuarioService.ActualizarUsuarioAsync(usuario);

        //        // ✅ Si la solicitud viene de AJAX → devolver JSON directamente
        //        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //        {
        //            return Json(new
        //            {
        //                success = apiResponse.Success,
        //                message = apiResponse.Message,
        //                detail = apiResponse.Data
        //            });
        //        }

        //        // ✅ Si es un submit normal → usar ViewData y mostrar modal
        //        if (apiResponse.Success)
        //        {
        //            ViewData["ModalSuccess"] = apiResponse.Message;
        //        }
        //        else
        //        {
        //            ViewData["ModalError"] = apiResponse.Message;
        //            if (apiResponse.Data != null)
        //                ViewData["ModalDetail"] = apiResponse.Data.ToString();
        //        }

        //        await CargarRolesAsync();
        //        return View("Editar", usuario);
        //    }
        //    catch (Exception ex)
        //    {
        //        var mensajeError = "Error interno del sistema al actualizar usuario";

        //        // AJAX → JSON
        //        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //            return Json(new { success = false, message = mensajeError, detail = ex.Message });

        //        // No AJAX → View
        //        ViewData["ModalError"] = mensajeError;
        //        ViewData["ModalDetail"] = ex.Message;
        //        await CargarRolesAsync();
        //        return View("Editar", usuario);
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

                // ✅ Obtener información de la sesión actual para bitácora
                var userSession = _authService.GetUserId();

                var apiResponse = await _usuarioService.ActualizarUsuarioAsync(usuario);

                // ✅ REGISTRAR EN BITÁCORA - EDICIÓN DE USUARIO
                if (apiResponse.Success && userSession.HasValue)
                {
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "EDITAR_USUARIO", "Usuarios", usuario.IdUsuario);
                }
                else if (!apiResponse.Success && userSession.HasValue)
                {
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EDITAR_USUARIO", "Usuarios", usuario.IdUsuario);
                }

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
                // ✅ REGISTRAR EN BITÁCORA - ERROR EN EDICIÓN
                var userSession = _authService.GetUserId();
                if (userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EXCEPCION_EDITAR_USUARIO", "Usuarios", usuario?.IdUsuario ?? 0);

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


        // ======== ACTIVAR / DESACTIVAR ========

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DesactivarUsuario(int id)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
        //        }


        //        var resultado = await _usuarioService.DesactivarUsuarioAsync(id);

        //        return Json(new
        //        {
        //            success = resultado,
        //            message = resultado ? "Usuario desactivado exitosamente" : "Error al desactivar el usuario"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error interno del sistema al desactivar usuario" });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
                }

                // ✅ Obtener información de la sesión actual para bitácora
                var userSession = _authService.GetUserId();

                var resultado = await _usuarioService.DesactivarUsuarioAsync(id);

                // ✅ REGISTRAR EN BITÁCORA
                if (userSession.HasValue)
                {
                    if (resultado)
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "DESACTIVAR_USUARIO", "Usuarios", id);
                    else
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_DESACTIVAR_USUARIO", "Usuarios", id);
                }

                return Json(new
                {
                    success = resultado,
                    message = resultado ? "Usuario desactivado exitosamente" : "Error al desactivar el usuario"
                });
            }
            catch (Exception ex)
            {
                // ✅ REGISTRAR EN BITÁCORA - ERROR
                var userSession = _authService.GetUserId();
                if (userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EXCEPCION_DESACTIVAR_USUARIO", "Usuarios", id);

                return Json(new { success = false, message = "Error interno del sistema al desactivar usuario" });
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ActivarUsuario(int id)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
        //        }

        //        var resultado = await _usuarioService.ActivarUsuarioAsync(id);

        //        return Json(new
        //        {
        //            success = resultado,
        //            message = resultado ? "Usuario activado exitosamente" : "Error al activar el usuario"
        //        });
        //    }
        //    catch
        //    {
        //        return Json(new { success = false, message = "Error interno del sistema al desactivar usuario" });

        //    }
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
                }

                // ✅ Obtener información de la sesión actual para bitácora
                var userSession = _authService.GetUserId();

                var resultado = await _usuarioService.ActivarUsuarioAsync(id);

                // ✅ REGISTRAR EN BITÁCORA
                if (userSession.HasValue)
                {
                    if (resultado)
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ACTIVAR_USUARIO", "Usuarios", id);
                    else
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_ACTIVAR_USUARIO", "Usuarios", id);
                }

                return Json(new
                {
                    success = resultado,
                    message = resultado ? "Usuario activado exitosamente" : "Error al activar el usuario"
                });
            }
            catch (Exception ex)
            {
                // ✅ REGISTRAR EN BITÁCORA - ERROR
                var userSession = _authService.GetUserId();
                if (userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EXCEPCION_ACTIVAR_USUARIO", "Usuarios", id);

                return Json(new { success = false, message = "Error interno del sistema al activar usuario" });
            }
        }

        //// ======== ASIGNAR / QUITAR ROLES ========

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AsignarRol([FromBody] AsignacionRolDto dto)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
        //        }

        //        if (dto.IdUsuario <= 0 || dto.IdRol <= 0)
        //        {
        //            return Json(new { success = false, message = "Datos inválidos para asignar rol" });
        //        }

        //        var (ok, mensaje) = await _rolService.AsignarRolAUsuarioAsync(dto);

        //        return Json(new
        //        {
        //            success = ok,
        //            message = mensaje ?? (ok ? "Rol asignado correctamente" : "Error al asignar el rol")
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error interno al asignar rol" });
        //    }
        //}

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

                // ✅ Obtener información de la sesión actual para bitácora
                var userSession = _authService.GetUserId();

                var (ok, mensaje) = await _rolService.AsignarRolAUsuarioAsync(dto);

                // ✅ REGISTRAR EN BITÁCORA
                if (userSession.HasValue)
                {
                    if (ok)
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ASIGNAR_ROL", "UsuarioRoles", dto.IdUsuario);
                    else
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_ASIGNAR_ROL", "UsuarioRoles", dto.IdUsuario);
                }

                return Json(new
                {
                    success = ok,
                    message = mensaje ?? (ok ? "Rol asignado correctamente" : "Error al asignar el rol")
                });
            }
            catch (Exception ex)
            {
                // ✅ REGISTRAR EN BITÁCORA - ERROR
                var userSession = _authService.GetUserId();
                if (userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EXCEPCION_ASIGNAR_ROL", "UsuarioRoles", dto?.IdUsuario ?? 0);

                return Json(new { success = false, message = "Error interno al asignar rol" });
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> QuitarRol([FromBody] AsignacionRolDto dto)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });
        //        }

        //        if (dto.IdUsuario <= 0 || dto.IdRol <= 0)
        //        {
        //            return Json(new { success = false, message = "Datos inválidos para quitar rol" });
        //        }

        //        var (ok, mensaje) = await _rolService.QuitarRolAUsuarioAsync(dto);

        //        return Json(new
        //        {
        //            success = ok,
        //            message = mensaje ?? (ok ? "Rol quitado correctamente" : "Error al quitar el rol")
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error interno al quitar rol" });
        //    }
        //}


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

                // ✅ Obtener información de la sesión actual para bitácora
                var userSession = _authService.GetUserId();

                var (ok, mensaje) = await _rolService.QuitarRolAUsuarioAsync(dto);

                // ✅ REGISTRAR EN BITÁCORA
                if (userSession.HasValue)
                {
                    if (ok)
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "QUITAR_ROL", "UsuarioRoles", dto.IdUsuario);
                    else
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_QUITAR_ROL", "UsuarioRoles", dto.IdUsuario);
                }

                return Json(new
                {
                    success = ok,
                    message = mensaje ?? (ok ? "Rol quitado correctamente" : "Error al quitar el rol")
                });
            }
            catch (Exception ex)
            {
                // ✅ REGISTRAR EN BITÁCORA - ERROR
                var userSession = _authService.GetUserId();
                if (userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EXCEPCION_QUITAR_ROL", "UsuarioRoles", dto?.IdUsuario ?? 0);

                return Json(new { success = false, message = "Error interno al quitar rol" });
            }
        }



        // ======== PERFIL USUARIO ========

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
        //public async Task<IActionResult> EditarPerfil(PerfilUsuarioDto perfilUsuario, bool DebeReautenticar)
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
        //            if (DebeReautenticar)
        //            {
        //                //TempData["SuccessMessage"] = "Tu perfil fue actualizado. Por seguridad, inicia sesión nuevamente.";
        //                // Cierra sesión y redirige al login
        //                await _authService.LogoutAsync();
        //                return RedirectToAction("Login", "Usuario");
        //            }

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

                // ✅ Obtener información de la sesión actual para bitácora
                var userSession = _authService.GetUserId();

                var actualizado = await _usuarioService.ActualizarPerfilsync(perfilUsuario);

                // ✅ REGISTRAR EN BITÁCORA
                if (userSession.HasValue)
                {
                    if (actualizado)
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "EDITAR_PERFIL", "Usuarios", perfilUsuario.idUsuario);
                    else
                        _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EDITAR_PERFIL", "Usuarios", perfilUsuario.idUsuario);
                }

                if (actualizado)
                {
                    if (DebeReautenticar)
                    {
                        // ✅ REGISTRAR EN BITÁCORA - REAUTENTICACIÓN REQUERIDA
                        if (userSession.HasValue)
                            _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "REAUTENTICACION_REQUERIDA", "Usuarios", perfilUsuario.idUsuario);

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
            catch (Exception ex)
            {
                // ✅ REGISTRAR EN BITÁCORA - ERROR
                var userSession = _authService.GetUserId();
                if (userSession.HasValue)
                    _ = _bitacoraService.RegistrarAccionAsync(userSession.Value, "ERROR_EXCEPCION_EDITAR_PERFIL", "Usuarios", perfilUsuario?.idUsuario ?? 0);

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


        public IActionResult ForgotPassword()
        {
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _usuarioService.ForgotPasswordAsync(model.Email);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return View(model);
        }

        // GET: Reset Password (cuando el usuario hace clic en el enlace del correo)
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            _logger.LogInformation($"Token recibido: {token}");


            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Token de recuperación inválido.";
                return RedirectToAction("ForgotPassword");
            }

            // Validar el token antes de mostrar el formulario
            var validationResult = await _usuarioService.ValidateResetTokenAsync(token);

            if (!validationResult.Success)
            {
                TempData["ErrorMessage"] = validationResult.Message ?? "El enlace de recuperación no es válido o ha expirado.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordRequest { Token = token };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState no es válido");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                _logger.LogWarning("Las contraseñas no coinciden");
                ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                return View(model);
            }

            try
            {
                _logger.LogInformation("Llamando a _usuarioService.ResetPasswordAsync");
                var result = await _usuarioService.ResetPasswordAsync(model.Token, model.NewPassword);
                _logger.LogInformation($"Resultado: Success={result.Success}, Message={result.Message}");

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Login", "Usuario");
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en ResetPassword POST");
                TempData["ErrorMessage"] = "Error interno del servidor.";
                return View(model);
            }
        }
    }
}
