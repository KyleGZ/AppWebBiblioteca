using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppWebBiblioteca.Controllers
{
    public class EmailSettingsController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailSettingsController> _logger;
        private readonly IAuthService _authService;

        public EmailSettingsController(IEmailService emailService, ILogger<EmailSettingsController> logger, IAuthService authService)
        {
            _emailService = emailService;
            _logger = logger;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if(!_authService.IsAuthenticated())
                    {
                    return RedirectToAction("Login", "Usuario");
                }
                var settings = await _emailService.GetSettingsAsync();
                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la vista de configuración de correo.");
                ViewBag.ErrorMessage = "Error al cargar la configuración de correo.";
                return View(new EmailSettings());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(EmailSettings model)
        {
            if(!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Usuario");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Hay errores en el formulario. Verifique los datos.";
                return View("Index", model);
            }

            var result = await _emailService.UpdateSettingsAsync(model);

            if (result)
            {
                TempData["Success"] = "Configuración de correo actualizada correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo actualizar la configuración. Revise el log para más detalles.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /EmailSettings/TestConnection
        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                if(!_authService.IsAuthenticated())
                {
                    return RedirectToAction("Login", "Usuario");
                }

                var success = await _emailService.TestConnectionAsync();

                if (success)
                    TempData["Success"] = "La conexión SMTP se probó exitosamente.";
                else
                    TempData["Error"] = "Falló la prueba de conexión SMTP.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error probando conexión SMTP.");
                TempData["Error"] = "Ocurrió un error al probar la conexión SMTP.";
                return RedirectToAction(nameof(Index));
            }
        }



    }
}
