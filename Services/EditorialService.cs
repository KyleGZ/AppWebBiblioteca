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

        // Registrar Editorial con ApiResponse
        public async Task<ApiResponse> RegistrarEditorialAsync(string nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Editorial/Registro";
                var payload = new { IdEditorial = 0, Nombre = nombre };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                if (!response.IsSuccessStatusCode)
                    return new ApiResponse { Success = false, Message = "Error en la comunicación con el API" };

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("success", out var okProp) && okProp.GetBoolean())
                {
                    string message = "Registro exitoso";
                    if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
                    {
                        message = messageProp.GetString();
                    }

                    object data = null;
                    if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
                    {
                        data = JsonSerializer.Deserialize<object>(dataProp.GetRawText());
                    }

                    return new ApiResponse { Success = true, Message = message, Data = data };
                }
                else
                {
                    string errorMessage = "Error en el registro";
                    if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
                    {
                        errorMessage = messageProp.GetString();
                    }

                    return new ApiResponse { Success = false, Message = errorMessage };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = $"Error interno: {ex.Message}" };
            }
        }

        // Editar Editorial con ApiResponse
        public async Task<ApiResponse> EditarEditorialAsync(int idEditorial, string nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Editorial/Editar";
                var payload = new { IdEditorial = idEditorial, Nombre = nombre };

                var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error HTTP: {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var apiResponse = new ApiResponse
                {
                    Success = root.TryGetProperty("success", out var okProp) && okProp.GetBoolean(),
                    Message = root.TryGetProperty("message", out var msgProp)
                             ? msgProp.GetString()
                             : "Operación completada"
                };

                // Opcional: incluir data si existe
                if (root.TryGetProperty("data", out var dataProp))
                {
                    apiResponse.Data = dataProp.GetRawText();
                }

                return apiResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }

        // Eliminar Editorial con ApiResponse
        public async Task<ApiResponse> EliminarEditorialAsync(int idEditorial)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Editorial/Eliminar?id={idEditorial}";
                var response = await _httpClient.DeleteAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error HTTP: {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var apiResponse = new ApiResponse
                {
                    Success = root.TryGetProperty("success", out var okProp) && okProp.GetBoolean(),
                    Message = root.TryGetProperty("message", out var msgProp)
                             ? msgProp.GetString()
                             : "Operación completada"
                };

                // Opcional: incluir data si existe
                if (root.TryGetProperty("data", out var dataProp))
                {
                    apiResponse.Data = dataProp.GetRawText();
                }

                return apiResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
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
        Task<ApiResponse> RegistrarEditorialAsync(string nombre);
        Task<ApiResponse> EditarEditorialAsync(int idEditorial, string nombre);
        Task<ApiResponse> EliminarEditorialAsync(int idEditorial);
        Task<PaginacionResponse<EditorialDto>> BuscarEditorialesRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
    }
}
