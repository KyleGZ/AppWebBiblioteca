namespace AppWebBiblioteca.Models
{
    // En Models/ObtenerLibroEditar.cs
    public class ObtenerLibroEditar
    {
        public int IdLibro { get; set; }
        public string Titulo { get; set; }
        public string ISBN { get; set; }
        public int EditorialId { get; set; }
        public int SeccionId { get; set; }
        public string Estado { get; set; }
        public string Descripcion { get; set; }
        public string PortadaUrl { get; set; }
        public List<AutorChipDto> Autores { get; set; } = new List<AutorChipDto>();
        public List<GeneroChipDto> Generos { get; set; } = new List<GeneroChipDto>();
    }

    // En Models/AutorChipDto.cs
    public class AutorChipDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    // En Models/GeneroChipDto.cs  
    public class GeneroChipDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
