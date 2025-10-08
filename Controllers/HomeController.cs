using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AppWebBiblioteca.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;

        public HomeController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Usuario");

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.UserRoles = _authService.GetUserRoles();

            return View();
        }


    }
}
