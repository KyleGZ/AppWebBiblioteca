﻿namespace AppWebBiblioteca.Models
{
    public class EditarUsuarioDto
    {
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Cedula { get; set; }
        public string? Password { get; set; }
        public string? Estado { get; set; }

    }
}
