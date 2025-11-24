using System.Text.Json;
using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class AutorService : IAutorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AutorService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AgregarTokenAutenticacion()
        {

            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");

            if (!string.IsNullOrEmpty(token))
            {

                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

        }

        public async Task<List<AutorDto>> ObtenerAutoresAsync(string? nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Autor/Lista-Autores?nombre={nombre}";
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var autores = await response.Content.ReadFromJsonAsync<List<AutorDto>>();
                    return autores ?? new List<AutorDto>();
                }
                return new List<AutorDto>();
            }
            catch
            {
                return new List<AutorDto>();
            }
        }

        public async Task<PaginacionResponse<AutorDto>> BuscarAutoresRapidaAsync(
         string termino,
         int pagina = 1,
          int resultadosPorPagina = 20)
        {
            try
            {
                // Si no hay término => listar paginado
                if (string.IsNullOrWhiteSpace(termino))
                {
                    AgregarTokenAutenticacion();
                    var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Autor/ListarViewAutor?pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                    var response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultado = await response.Content.ReadFromJsonAsync<PaginacionResponse<AutorDto>>();
                        if (resultado != null && resultado.Success)
                        {
                            return resultado;
                        }
                        return new PaginacionResponse<AutorDto>
                        {
                            Success = false,
                            Message = "No se pudieron obtener los autores"
                        };
                    }

                    return new PaginacionResponse<AutorDto>
                    {
                        Success = false,
                        Message = $"Error al obtener los autores: {response.StatusCode}"
                    };
                }

                AgregarTokenAutenticacion();
                // Con término => búsqueda paginada
                var buscarUrl =
                    $"{_configuration["ApiSettings:BaseUrl"]}/Autor/Busqueda-Autor" +
                    $"?termino={Uri.EscapeDataString(termino.Trim())}" +
                    $"&pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";

                var buscarResponse = await _httpClient.GetAsync(buscarUrl);

                if (buscarResponse.IsSuccessStatusCode)
                {
                    var result = await buscarResponse.Content.ReadFromJsonAsync<PaginacionResponse<AutorDto>>();
                    return result ?? new PaginacionResponse<AutorDto>
                    {
                        Success = false,
                        Message = "No se pudieron procesar los resultados de la búsqueda"
                    };
                }

                return new PaginacionResponse<AutorDto>
                {
                    Success = false,
                    Message = $"Error en la búsqueda: {buscarResponse.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<AutorDto>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }


        public async Task<ApiResponse> RegistrarAutorAsync(string nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Autor/Registro";
                var payload = new { IdAutor = 0, Nombre = nombre };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, json);
                }

                // Procesar respuesta exitosa usando ExtractFromJson
                return ExtractFromJson(json);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
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


        public async Task<ApiResponse> EditarAutorAsync(int idAutor, string nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Autor/Editar";
                var payload = new { IdAutor = idAutor, Nombre = nombre };

                var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, json);
                }

                // Procesar respuesta exitosa usando ExtractFromJson
                return ExtractFromJson(json);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
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

        public async Task<ApiResponse> EliminarAutorAsync(int idAutor)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Autor/Eliminar?id={idAutor}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, json);
                }

                // Procesar respuesta exitosa usando ExtractFromJson
                return ExtractFromJson(json);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
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




        private async Task<ApiResponse> HandleErrorResponse(HttpResponseMessage response, string json)
        {
            try
            {
                // Intentar deserializar como ApiResponse
                var errorResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    return errorResponse;
                }

                // Fallback: extraer mensaje manualmente
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var errorMessage = root.TryGetProperty("message", out var msg)
                    ? msg.GetString()
                    : root.TryGetProperty("title", out var title)
                        ? title.GetString()
                        : $"Error del servidor (HTTP {(int)response.StatusCode})";

                return new ApiResponse { Success = false, Message = errorMessage };
            }
            catch
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error en la comunicación con el API (HTTP {(int)response.StatusCode})"
                };
            }
        }

        private ApiResponse ExtractFromJson(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                bool success = root.TryGetProperty("success", out var s) && s.GetBoolean();
                string message = root.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : (success ? "Operación exitosa" : "Error en la operación");

                object data = null;
                if (root.TryGetProperty("data", out var d) && d.ValueKind != JsonValueKind.Null)
                {
                    data = JsonSerializer.Deserialize<object>(d.GetRawText());
                }

                return new ApiResponse { Success = success, Message = message, Data = data };
            }
            catch
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Formato de respuesta inválido"
                };
            }
        }
        public async Task<int> ObtenerIdAutor(string nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Autor/Get-autor?nombre={Uri.EscapeDataString(nombre)}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return 0;

                // Leer contenido como string
                var content = await response.Content.ReadAsStringAsync();

                // Intentar convertir directamente a int
                if (int.TryParse(content, out int idEditorial))
                    return idEditorial;

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ID de la editorial por nombre: {ex.Message}");
                return 0;
            }
        }

    }

    public interface IAutorService
    {
        Task<List<AutorDto>> ObtenerAutoresAsync(string? nombre);
        Task<PaginacionResponse<AutorDto>> BuscarAutoresRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);

        Task<ApiResponse> RegistrarAutorAsync(string nombre);
        Task<ApiResponse> EditarAutorAsync(int idAutor, string nombre);
        Task<ApiResponse> EliminarAutorAsync(int idAutor);
        Task<int> ObtenerIdAutor(string nombre);

    }
}
