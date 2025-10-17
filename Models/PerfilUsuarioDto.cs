namespace AppWebBiblioteca.Models
{
    public class PerfilUsuarioDto
    {
        public int idUsuario { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public string? Password { get; set; }
    }

}
