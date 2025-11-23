using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Policy = "AdminOnly")]
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
                var updateModel = new UpdateEmailSettings
                {
                    FromName = settings.FromName,
                    FromEmail = settings.FromEmail,
                    SmtpHost = settings.SmtpHost,
                    SmtpPort = settings.SmtpPort,
                    UseStartTls = settings.UseStartTls,
                    Username = settings.Username,
                    Password = string.Empty // No llenar el campo de contraseña por seguridad
                };

                return View(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la vista de configuración de correo.");
                ViewBag.ErrorMessage = "Error al cargar la configuración de correo.";
                return View(new EmailSettings());
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Update(UpdateEmailSettings settings)
        //{
        //    try
        //    {
        //        if (!_authService.IsAuthenticated())
        //        {
        //            return RedirectToAction("Login", "Usuario");
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            TempData["ErrorMessage"] = "Hay errores en el formulario. Verifique los datos.";
        //            return View("Index", settings);
        //        }

        //        var result = await _emailService.UpdateSettingsAsync(settings);

        //        if (result.Success)
        //        {
        //            TempData["SuccessMessage"] = result.Message ?? "Configuración de correo actualizada correctamente.";
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = result.Message ?? "No se pudo actualizar la configuración.";
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error inesperado al actualizar configuración de correo");
        //        TempData["ErrorMessage"] = "Error inesperado al procesar la solicitud.";
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(UpdateEmailSettings settings)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToAction("Login", "Usuario");
                }

                if (!ModelState.IsValid)
                {
                    // Recopilar todos los mensajes de error específicos
                    var errorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    TempData["ErrorMessage"] = "Hay errores en el formulario: " + string.Join("; ", errorMessages);
                    return View("Index", settings);
                }

                var result = await _emailService.UpdateSettingsAsync(settings);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message ?? "Configuración de correo actualizada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "No se pudo actualizar la configuración.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar configuración de correo");
                TempData["ErrorMessage"] = "Error inesperado al procesar la solicitud.";
                return RedirectToAction(nameof(Index));
            }
        }



        // GET: /EmailSettings/TestConnection
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            var result = await _emailService.TestConnectionAsync();

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("TestConnectionAjax")]
        public async Task<IActionResult> TestConnectionAjax()
        {
            var result = await _emailService.TestConnectionAsync();
            return Json(result);
        }


    }
}
