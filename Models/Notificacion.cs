namespace AppWebBiblioteca.Models
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }

        public int IdUsuario { get; set; }

        public string Asunto { get; set; } = null!;

        public string Mensaje { get; set; } = null!;

        public DateTime FechaEnvio { get; set; }

        public string Estado { get; set; }
    }
}
