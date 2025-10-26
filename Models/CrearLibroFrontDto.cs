
namespace AppWebBiblioteca.Models
{
    public class CrearLibroFrontDto
    {
        public string Titulo { get; set; }
        public string ISBN { get; set; }
        public int EditorialId { get; set; }
        public int SeccionId { get; set; }
        public string Estado { get; set; }
        public string? Descripcion { get; set; }
        public IFormFile? ImagenArchivo { get; set; }
        public string GenerosSeleccionados { get; set; } // Cadena separada por comas: "1,2,3"
        public string AutoresSeleccionados { get; set; } // Cadena separada por comas: "1,2,3"
    }
}
