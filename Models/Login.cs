using System.ComponentModel.DataAnnotations;

namespace AppWebBiblioteca.Models
{
    public class Login
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string password { get; set; }
    }
}
