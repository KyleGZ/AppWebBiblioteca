using System.ComponentModel.DataAnnotations;

namespace AppWebBiblioteca.Models
{
    public class ReservaDto
    {
        //[Required(ErrorMessage = "El Usuario es obligatorio.")]
        public int IdUsuario { get; set; }

        //[Required(ErrorMessage = "El Libro es obligatorio.")]
        public int IdLibro { get; set; }

    }
}
