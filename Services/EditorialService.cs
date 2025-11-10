using AppWebBiblioteca.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class EditorialService : IEditorialService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EditorialService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<EditorialDto>> ObtenerEditorialesAsync(string? nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Editorial/Lista-Editoriales?nombre={nombre}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var editoriales = await response.Content.ReadFromJsonAsync<List<EditorialDto>>();
                    return editoriales ?? new List<EditorialDto>();
                }
                return new List<EditorialDto>();
            }
            catch
            {
                return new List<EditorialDto>();
            }
        }

        //Registrar Editorial
        public async Task<int> RegistrarEditorialAsync(string nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Editorial/Registro";
                var payload = new { IdEditorial = 0, Nombre = nombre };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                if (!response.IsSuccessStatusCode) return 0;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // success debe ser true y data.idEditorial debe existir
                if (root.TryGetProperty("success", out var okProp) && okProp.GetBoolean() &&
                    root.TryGetProperty("data", out var dataProp) &&
                    dataProp.ValueKind == JsonValueKind.Object &&
                    dataProp.TryGetProperty("idEditorial", out var idProp) &&
                    idProp.TryGetInt32(out var id))
                {
                    return id;
                }

                // Fallback (por compatibilidad)
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // Editar Editorial -> ApiResponse { success, message, data? }
        public async Task<bool> EditarEditorialAsync(int idEditorial, string nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Editorial/Editar";
                var payload = new { IdEditorial = idEditorial, Nombre = nombre };

                var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return root.TryGetProperty("success", out var okProp) && okProp.GetBoolean();
            }
            catch
            {
                return false;
            }
        }

        // Eliminar Editorial -> ApiResponse { success, message }
        public async Task<bool> EliminarEditorialAsync(int idEditorial)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Editorial/Eliminar?id={idEditorial}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return root.TryGetProperty("success", out var okProp) && okProp.GetBoolean();
            }
            catch
            {
                return false;
            }
        }

        // Búsqueda y paginación -> ApiResponse { success, message, data: PaginacionResponse<EditorialDto> }
        public async Task<PaginacionResponse<EditorialDto>> BuscarEditorialesRapidaAsync(
            string termino, int pagina = 1, int resultadosPorPagina = 20)
        {
            try
            {
                string apiUrl;
                if (string.IsNullOrWhiteSpace(termino))
                {
                    apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Editorial/ListarViewEditorial?pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                }
                else
                {
                    apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Editorial/Busqueda-Editorial?termino={Uri.EscapeDataString(termino)}&pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                }

                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return new PaginacionResponse<EditorialDto>
                    {
                        Success = false,
                        Message = $"Error al obtener las editoriales: {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Validar success
                if (!(root.TryGetProperty("success", out var okProp) && okProp.GetBoolean()))
                {
                    var msg = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Error en la respuesta";
                    return new PaginacionResponse<EditorialDto> { Success = false, Message = msg ?? "Error en la respuesta" };
                }

                // Extraer data (que es un PaginacionResponse<EditorialDto>)
                if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                {
                    var paginado = dataProp.Deserialize<PaginacionResponse<EditorialDto>>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (paginado != null)
                    {
                        paginado.Success = true; // asegurar flag
                        return paginado;
                    }
                }

                // Fallback si no se pudo deserializar
                return new PaginacionResponse<EditorialDto>
                {
                    Success = false,
                    Message = "No se pudieron procesar los resultados"
                };
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<EditorialDto>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }
    }

    public interface IEditorialService
    {
        Task<List<EditorialDto>> ObtenerEditorialesAsync(string? nombre);
        Task<int> RegistrarEditorialAsync(string nombre);
        Task<bool> EditarEditorialAsync(int idEditorial, string nombre);
        Task<bool> EliminarEditorialAsync(int idEditorial);
        Task<PaginacionResponse<EditorialDto>> BuscarEditorialesRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
    }
}
