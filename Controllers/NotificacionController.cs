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

        public async Task<IActionResult> ObtenerNotificaciones()
        {
            try
            {

                if (!_authService.IsAuthenticated())
                    return Json(new { success = false, message = "Debe iniciar sesión para realizar esta acción" });

                var idUsuario = _authService.GetUserId();
                if (idUsuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado" });

                var notificaciones = await _notificacionService.ObtenerNotificacionesAsync(idUsuario.Value);

                if (notificaciones == null)
                    return Json(new { success = false, message = "No se pudieron obtener las notificaciones" });

                return Json(new { success = true, data = notificaciones});
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
