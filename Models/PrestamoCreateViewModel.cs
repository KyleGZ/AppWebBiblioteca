using System;
using System.ComponentModel.DataAnnotations;

namespace AppWebBiblioteca.Models
{
    public class PrestamoCreateViewModel
    {
        [Required(ErrorMessage = "El lector es obligatorio")]
        public string UsuarioId { get; set; } = null!;

        [Required(ErrorMessage = "El libro es obligatorio")]
        public int LibroId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaPrestamo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }
}