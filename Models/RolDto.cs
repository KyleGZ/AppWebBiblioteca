using System.Text.Json.Serialization;

namespace AppWebBiblioteca.Models
{
    public class RolDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class ApiMensajeResponse
    {
        [JsonPropertyName("mensaje")]
        public string? Mensaje { get; set; }
    }
}
