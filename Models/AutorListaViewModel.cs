namespace AppWebBiblioteca.Models
{
    public class AutorListaViewModel
    {
        public string Mensaje { get; set; } = string.Empty;
        public int TotalAutores { get; set; }
        public List<AutorDto> Autores { get; set; } = new();
        public string? Filtro { get; set; }
    }
}
