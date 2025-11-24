using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebBiblioteca.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly ILogger<NotificacionController> _logger;
        private readonly INotificacionService _notificacionService;
        private readonly IAuthService _authService;

        public NotificacionController(ILogger<NotificacionController> logger, INotificacionService notificacionService, IAuthService authService)
        {
            _logger = logger;
            _notificacionService = notificacionService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerNotificaciones()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });

                var idUsuario = _authService.GetUserId();
                if (idUsuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado" });

                // Ahora ObtenerNotificacionesAsync devuelve List<NotificacionView>
                var notificaciones = await _notificacionService.ObtenerNotificacionesAsync(idUsuario.Value);

                // Retornar la lista dentro de data
                return Json(new { success = true, data = notificaciones });
            }
            catch (Exception ex)
            {
                // Considera loguear el error en lugar de ex.Message en producción
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarcarComoLeida(int idNotificacion)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Debe iniciar sesión"
                    });
                }

                var resultado = await _notificacionService.MarcarComoLeidaAsync(idNotificacion);

                // NORMALIZAR el retorno para que SIEMPRE use camelCase
                return Json(new
                {
                    success = resultado.Success, 
                    message = resultado.Message ?? "Operación completada"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error inesperado: {ex.Message}"
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> MarcarTodasComoLeidas()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión" });

                var idUsuario = _authService.GetUserId();
                if (idUsuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado" });

                var resultado = await _notificacionService.MarcarTodasComoLeidasAsync(idUsuario.Value);

                return Json(new
                {
                    success = resultado.Success,
                    message = resultado.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Metodo para eliminar todas las notificaciones de un usuario

        public async Task<IActionResult> EliminarTodasNotificaciones()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión" });

                var idUsuario = _authService.GetUserId();
                if (idUsuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado" });
                var resultado = await _notificacionService.EliminarTodasAsync(idUsuario.Value);
                return Json(new
                {
                    success = resultado.Success,
                    message = resultado.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
