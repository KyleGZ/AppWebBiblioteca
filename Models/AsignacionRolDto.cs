using System.Text.Json.Serialization;

namespace AppWebBiblioteca.Models
{
    public class AsignacionRolDto
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("idRol")]
        public int IdRol { get; set; }
    }
}
