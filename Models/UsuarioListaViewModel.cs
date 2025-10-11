namespace AppWebBiblioteca.Models
{
    public class UsuarioListaViewModel
    {
        public int IdUsuario { get; set; }
        public string Cedula { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public List<int> RolesIds { get; set; } = new();
        public List<string> Roles { get; set; } = new();
    }


    public class ApiResponse<T>
    {
        public string Mensaje { get; set; } = null!;
        public int TotalUsuarios { get; set; }
        public List<T> Usuarios { get; set; } = new List<T>();
    }
}
