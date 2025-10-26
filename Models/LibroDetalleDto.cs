﻿namespace AppWebBiblioteca.Models
{
    public class LibroDetalleDto
    {
        public string Isbn { get; set; }
        public string Titulo { get; set; }
        public List<string> Autor { get; set; } = new();
        public List<string> Genero { get; set; } = new();
        public string Editorial { get; set; }
        public string Seccion { get; set; }
        public string Descripcion { get; set; }

        public string Portada { get; set; }
    }
}
