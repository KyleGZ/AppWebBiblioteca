
using System.ComponentModel.DataAnnotations;

namespace AppWebBiblioteca.Models
{
    public class CrearLibroFrontDto
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El ISBN es obligatorio.")]
        public string ISBN { get; set; }
        [Required(ErrorMessage = "La editorial es obligatoria.")]
        public int EditorialId { get; set; }
        [Required(ErrorMessage = "La sección es obligatoria.")]
        public int SeccionId { get; set; }
        public string Estado { get; set; }
        public string? Descripcion { get; set; }
        public IFormFile? ImagenArchivo { get; set; }
        public string GenerosSeleccionados { get; set; } // Cadena separada por comas: "1,2,3"
        public string AutoresSeleccionados { get; set; } // Cadena separada por comas: "1,2,3"
    }
}
