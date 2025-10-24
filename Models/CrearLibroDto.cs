﻿namespace AppWebBiblioteca.Models
{
    public class CrearLibroDto
    {
        public string Titulo { get; set; } = null!;
        public string Isbn { get; set; } = null!;
        public int IdEditorial { get; set; }
        public int IdSeccion { get; set; }
        public string? Estado { get; set; }
        public string? Descripcion { get; set; }
        public string? PortadaUrl { get; set; }
        public List<int> IdAutores { get; set; } = new List<int>();
        public List<int>? IdGeneros { get; set; } = new List<int>();
    }
}
