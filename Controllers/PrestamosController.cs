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
        // LISTA TEMPORAL para guardar préstamos
        private static List<dynamic> _prestamos = new List<dynamic>();


        // Datos de ejemplo para poblar selects (reemplaza por tus servicios/repositorios)
        private static readonly List<(string Id, string Nombre)> _usuarios = new()
        {
            ("1", "Juan Pérez"),
            ("2", "María Gómez"),
            ("3", "Miguel de Cervantes")
        };

        private static readonly List<(int Id, string Titulo)> _libros = new()
        {
            (112, "Cien años de soledad"),
            (118, "Don Quijote de la Mancha"),
            (115, "El Principito")
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

//-----------------------------------------------------------------------------------------------------------------------------------------------------------

       
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

            try
            {
                // 1. CREAR PRÉSTAMO
                var nuevoPrestamo = new
                {
                    Id = DateTime.Now.Ticks, // ID único
                    UsuarioId = model.UsuarioId,
                    LibroId = model.LibroId,
                    FechaPrestamo = model.FechaPrestamo,
                    FechaVencimiento = model.FechaVencimiento,
                    Observaciones = model.Observaciones,
                    Estado = "Activo"
                };

                //Guardar en lista temporal
                _prestamos.Add(nuevoPrestamo);

                return Ok(new
                {
                    success = true,
                    message = "Préstamo registrado correctamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
    
        }//fin del POST

        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult GetPrestamos()
        {
            // Retornar todos los préstamos guardados
            return Json(_prestamos);
        }



    }//fin del public
}//fin del namespace


