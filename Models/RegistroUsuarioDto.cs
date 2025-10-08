namespace AppWebBiblioteca.Models
{
    public class RegistroUsuarioDto
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Cedula { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
