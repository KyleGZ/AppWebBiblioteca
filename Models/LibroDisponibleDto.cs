namespace AppWebBiblioteca.Models
{
    public class LibroDisponibleDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }
}