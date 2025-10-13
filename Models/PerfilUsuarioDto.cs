namespace AppWebBiblioteca.Models
{
    public class PerfilUsuarioDto
    {
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Cedula { get; set; }
    }

    public class PerfilUsuarioUpdateDto
    {
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        
        public string PasswordActual { get; set; }
        public string NuevoPassword { get; set; }
    }
}
