using AppWebBiblioteca.Models;
using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AppWebBiblioteca.Controllers
{
  
    [Authorize]
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
           
            ViewBag.UserRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
