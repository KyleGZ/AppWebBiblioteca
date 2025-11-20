using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class ReservasController : Controller
    {
        private readonly IReservaService _reservaService;
        private readonly IAuthService _authService;
        private readonly ILibroService _libroService;

        public ReservasController(IReservaService reservaService, IAuthService authService, ILibroService libroService)
        {
            _reservaService = reservaService;
            _authService = authService;
            _libroService = libroService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string termino = "", int pagina = 1, int resultadosPorPagina = 10, string tipoVista = "activas")
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Usuario");

                // OBTENER ID DEL USUARIO LOGUEADO
                var userId = _authService.GetUserId();
                if (userId == null)
                    return RedirectToAction("Login", "Usuario");

                PaginacionResponse<ReservaResponseDto> resultado;

                // DECIDIR QUÉ DATOS CARGAR SEGÚN EL TIPO DE VISTA
                if (tipoVista == "historicas")
                {
                    resultado = await _reservaService.ObtenerReservasHistoricasAsync(termino, pagina, resultadosPorPagina, userId.Value);
                    ViewBag.TipoVista = "historicas";
                    ViewBag.TituloSeccion = "Reservas Históricas";
                }
                else
                {
                    resultado = await _reservaService.ObtenerReservasActivasAsync(termino, pagina, resultadosPorPagina, userId.Value);
                    ViewBag.TipoVista = "activas";
                    ViewBag.TituloSeccion = "Reservas Activas";
                }

                // Obtener conteo de reservas activas del usuario
                var reservasActivasCount = await _reservaService.ObtenerConteoReservasActivasAsync(userId.Value);
                ViewBag.ReservasActivasCount = reservasActivasCount;

                ViewBag.TerminoBusqueda = termino ?? string.Empty;
                ViewBag.PaginaActual = pagina;
                ViewBag.ResultadosPorPagina = resultadosPorPagina;

                return View(resultado);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la lista de reservas";
                ViewBag.ReservasActivasCount = 0;
                ViewBag.TipoVista = "activas";
                ViewBag.TituloSeccion = "Reservas Activas";

                return View(new PaginacionResponse<ReservaResponseDto>
                {
                    Success = false,
                    Message = "Error al cargar las reservas",
                    Data = new List<ReservaResponseDto>(),
                    Pagination = new PaginationInfo
                    {
                        PaginaActual = pagina,
                        ResultadosPorPagina = resultadosPorPagina,
                        TotalResultados = 0,
                        TotalPaginas = 0
                    }
                });
            }
        }


        //Este es el que sirve
        //[HttpGet]
        //public async Task<IActionResult> Index(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //            return RedirectToAction("Login", "Usuario");

        //        // OBTENER ID DEL USUARIO LOGUEADO
        //        var userId = _authService.GetUserId();
        //        if (userId == null)
        //            return RedirectToAction("Login", "Usuario");

        //        // ✅ MODIFICADO: Pasar el userId al servicio
        //        PaginacionResponse<ReservaResponseDto> resultado =
        //            await _reservaService.ObtenerReservasAsync(termino, pagina, resultadosPorPagina, userId.Value);

        //        // ✅ NUEVO: Obtener conteo de reservas activas del usuario
        //        var reservasActivasCount = await _reservaService.ObtenerConteoReservasActivasAsync(userId.Value);
        //        ViewBag.ReservasActivasCount = reservasActivasCount;

        //        ViewBag.TerminoBusqueda = termino ?? string.Empty;
        //        ViewBag.PaginaActual = pagina;
        //        ViewBag.ResultadosPorPagina = resultadosPorPagina;

        //        return View(resultado);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Error al cargar la lista de reservas";
        //        ViewBag.ReservasActivasCount = 0; // Valor por defecto en caso de error
        //        return View(new PaginacionResponse<ReservaResponseDto>
        //        {
        //            Success = false,
        //            Message = "Error al cargar las reservas",
        //            Data = new List<ReservaResponseDto>(),
        //            Pagination = new PaginationInfo
        //            {
        //                PaginaActual = pagina,
        //                ResultadosPorPagina = resultadosPorPagina,
        //                TotalResultados = 0,
        //                TotalPaginas = 0
        //            }
        //        });
        //    }
        //}



        //[HttpGet]
        //public async Task<IActionResult> Index(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //            return RedirectToAction("Login", "Usuario");

        //        PaginacionResponse<ReservaResponseDto> resultado =
        //            await _reservaService.ObtenerReservasAsync(termino, pagina, resultadosPorPagina);

        //        ViewBag.TerminoBusqueda = termino ?? string.Empty;
        //        ViewBag.PaginaActual = pagina;
        //        ViewBag.ResultadosPorPagina = resultadosPorPagina;

        //        return View(resultado);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Error al cargar la lista de reservas";
        //        return View(new PaginacionResponse<ReservaResponseDto>
        //        {
        //            Success = false,
        //            Message = "Error al cargar las reservas",
        //            Data = new List<ReservaResponseDto>(),
        //            Pagination = new PaginationInfo
        //            {
        //                PaginaActual = pagina,
        //                ResultadosPorPagina = resultadosPorPagina,
        //                TotalResultados = 0,
        //                TotalPaginas = 0
        //            }
        //        });
        //    }
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarReserva([FromBody] SimpleReserva model, string? returnUrl = null)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });

                // OBTENER ID DEL USUARIO LOGUEADO AUTOMÁTICAMENTE
                var userId = _authService.GetUserId();
                if (userId == null)
                    return Json(new { success = false, message = "No se pudo identificar al usuario" });

                // DEBUG: Verificar qué estamos recibiendo
                Console.WriteLine($"🔍 DEBUG Controlador: Recibido IdLibro: {model?.IdLibro}");

                if (model == null || model.IdLibro <= 0)
                {
                    return Json(new { success = false, message = "ID de libro inválido" });
                }

                var reservaDto = new ReservaDto
                {
                    IdUsuario = userId.Value,
                    IdLibro = model.IdLibro,
                };

                Console.WriteLine($"🔍 DEBUG Controlador: Enviando a servicio - UsuarioId: {userId.Value}, LibroId: {model.IdLibro}");

                var resultado = await _reservaService.RegistrarReservaAsync(reservaDto);

                if (resultado.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Reserva creada exitosamente"
                    });
                }
                else
                {
                    return Json(new { success = false, message = resultado.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 DEBUG Controlador: Excepción - {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "Error al crear la reserva: " + ex.Message
                });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarReservaLibro([FromBody] int idLibro)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });

                var userId = _authService.GetUserId();
                if (userId == null)
                    return Json(new { success = false, message = "No se pudo identificar al usuario" });

                
                if (idLibro == null || idLibro <= 0)
                {
                    return Json(new { success = false, message = "ID de libro inválido" });
                }

                var reservaDto = new ReservaDto
                {
                    IdUsuario = userId.Value,
                    IdLibro = idLibro,
                };

                Console.WriteLine($"🔍 DEBUG Controlador: Enviando a servicio - UsuarioId: {userId.Value}, LibroId: {idLibro}");

                var resultado = await _reservaService.RegistrarReservaAsync(reservaDto);

                if (resultado.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Reserva creada exitosamente"
                    });
                }
                else
                {
                    return Json(new { success = false, message = resultado.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 DEBUG Controlador: Excepción - {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "Error al crear la reserva: " + ex.Message
                });
            }
        }


        // AGREGAR ESTA CLASE TEMPORAL
        public class SimpleReserva
        {
            public int IdLibro { get; set; }
        }


        [HttpGet]
        public async Task<IActionResult> BuscarLibrosParaReserva(string termino = "", int pagina = 1, int resultadosPorPagina = 10)
        {
            try
            {
                var resultado = await _libroService.BuscarLibrosRapidaAsync(termino, pagina, resultadosPorPagina);

                // Mostrar solo libros DISPONIBLES para reserva
                if (resultado.Success && resultado.Data != null)
                {
                    var librosDisponibles = resultado.Data
                        .Where(l => l.Estado == "Disponible")  // Solo disponibles
                        .ToList();

                    resultado.Data = librosDisponibles;
                    resultado.Pagination.TotalResultados = librosDisponibles.Count;
                }

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new PaginacionResponse<LibroListaView>
                {
                    Success = false,
                    Message = "Error al buscar libros",
                    Data = new List<LibroListaView>(),
                    Pagination = new PaginationInfo()
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarReserva(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "No autenticado" });

                var reserva = await _reservaService.ObtenerReservaIDAsync(id);
                if (reserva == null)
                {
                    return Json(new { success = false, message = "No se encontró la reserva." });
                }

                return Json(new { success = true, data = reserva });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar la reserva." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, string? returnUrl = null)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Id inválido." });
            }

            try
            {
                var resultado = await _reservaService.EliminarReservaAsync(id);

                if (resultado.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = resultado.Message ?? "Reserva eliminada correctamente."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = resultado.Message ?? "No se pudo eliminar la reserva."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al eliminar reserva: {ex.Message}"
                });
            }
        }
    }
}