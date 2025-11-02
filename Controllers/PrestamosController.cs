using System;
using System.Collections.Generic;
using System.Linq;
using AppWebBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppWebBiblioteca.Controllers
{
    [Authorize] // Opcional: restringe a usuarios autenticados
    public class PrestamosController : Controller
    {
        // Datos de ejemplo para poblar selects (reemplaza por tus servicios/repositorios)
        private static readonly List<(string Id, string Nombre)> _usuarios = new()
        {
            ("1", "Juan Pérez"),
            ("2", "María Gómez"),
            ("3", "Luis Rodríguez")
        };

        private static readonly List<(int Id, string Titulo)> _libros = new()
        {
            (100, "Cien años de soledad"),
            (101, "Don Quijote de la Mancha"),
            (102, "El Principito")
        };

        // GET: /Prestamos
        public IActionResult Index()
        {
            ViewData["Title"] = "Préstamos y Devoluciones";
            ViewData["PageTitle"] = "Préstamos y Devoluciones";

            ViewBag.Usuarios = _usuarios
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Nombre })
                .ToList();

            ViewBag.Libros = _libros
                .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Titulo })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PrestamoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kv => kv.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new { success = false, message = "Validación fallida", errors });
            }

            // TODO: Persistir el préstamo en BD aquí
            // var nuevoPrestamoId = _servicioPrestamos.Crear(model);

            return Ok(new
            {
                success = true,
                message = "Préstamo registrado correctamente."
                //, id = nuevoPrestamoId
            });
        }
    }
}

