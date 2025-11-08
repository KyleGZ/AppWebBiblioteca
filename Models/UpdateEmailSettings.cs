using System.ComponentModel.DataAnnotations;

namespace AppWebBiblioteca.Models
{
    public class UpdateEmailSettings
    {
        [Required(ErrorMessage = "El nombre del remitente es obligatorio")]
        public string FromName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo del remitente es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string FromEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El servidor SMTP es obligatorio")]
        public string SmtpHost { get; set; } = string.Empty;

        [Required(ErrorMessage = "El puerto SMTP es obligatorio")]
        [Range(1, 65535, ErrorMessage = "El puerto debe ser válido (1-65535)")]
        public int SmtpPort { get; set; }

        [Required(ErrorMessage = "Debe especificar si utiliza STARTTLS")]
        public bool UseStartTls { get; set; }

        [Required(ErrorMessage = "El nombre de usuario SMTP es obligatorio")]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
