namespace AppWebBiblioteca.Models
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public bool Resultado { get; set; }
        public string Msj { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public List<string> Roles { get; set; }
    }
}
