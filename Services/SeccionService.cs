using System.Text.Json;
using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class SeccionService : ISeccionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SeccionService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<SeccionDto>> ObtenerSeccionesAsync(string? nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Seccion/Lista-Secciones?nombre={nombre}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var secciones = await response.Content.ReadFromJsonAsync<List<SeccionDto>>();
                    return secciones ?? new List<SeccionDto>();
                }
                return new List<SeccionDto>();
            }
            catch
            {
                return new List<SeccionDto>();
            }
        }

        public async Task<ApiResponse> RegistrarSeccionAsync(string nombre, string ubicacion)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Seccion/Registro";
                var payload = new { IdSeccion = 0, Nombre = nombre, Ubicacion = ubicacion };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                var json = await response.Content.ReadAsStringAsync();

                // Debug opcional
                // Console.WriteLine($"API Seccion Response - Status: {response.StatusCode}, Body: {json}");

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


        public async Task<ApiResponse> EditarSeccionAsync(int idSeccion, string nombre, string ubicacion)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Seccion/Editar";
                var payload = new { IdSeccion = idSeccion, Nombre = nombre, Ubicacion = ubicacion };

                var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);
                var json = await response.Content.ReadAsStringAsync();

                // Debug opcional
                // Console.WriteLine($"API Edit Seccion Response - Status: {response.StatusCode}, Body: {json}");

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


        public async Task<ApiResponse> EliminarSeccionAsync(int idSeccion)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Seccion/Eliminar?id={idSeccion}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                var json = await response.Content.ReadAsStringAsync();

                // Debug opcional
                // Console.WriteLine($"API Delete Seccion Response - Status: {response.StatusCode}, Body: {json}");

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



        // Búsqueda y paginación
        public async Task<PaginacionResponse<SeccionDto>> BuscarSeccionesRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20)
        {
            try
            {
                AgregarTokenAutenticacion();
                string apiUrl;
                if (string.IsNullOrWhiteSpace(termino))
                {
                    apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Seccion/ListarViewSeccion?pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                }
                else
                {
                    apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Seccion/Busqueda-Seccion?termino={Uri.EscapeDataString(termino)}&pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                }

                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return new PaginacionResponse<SeccionDto>
                    {
                        Success = false,
                        Message = $"Error al obtener las secciones: {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!(root.TryGetProperty("success", out var okProp) && okProp.GetBoolean()))
                {
                    var msg = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Error en la respuesta";
                    return new PaginacionResponse<SeccionDto> { Success = false, Message = msg ?? "Error en la respuesta" };
                }

                if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                {
                    var paginado = dataProp.Deserialize<PaginacionResponse<SeccionDto>>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (paginado != null)
                    {
                        paginado.Success = true;
                        return paginado;
                    }
                }

                return new PaginacionResponse<SeccionDto>
                {
                    Success = false,
                    Message = "No se pudieron procesar los resultados"
                };
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<SeccionDto>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
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
        public async Task<int> ObtenerIdSeccion(string nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Seccion/Get-seccion?nombre={Uri.EscapeDataString(nombre)}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return 0;

                // Leer contenido como string
                var content = await response.Content.ReadAsStringAsync();

                // Intentar convertir directamente a int
                if (int.TryParse(content, out int idSeccion))
                    return idSeccion;

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ID de la Seccion por nombre: {ex.Message}");
                return 0;
            }
        }

    }

    public interface ISeccionService
    {
        Task<List<SeccionDto>> ObtenerSeccionesAsync(string? nombre);
        Task<ApiResponse> RegistrarSeccionAsync(string nombre, string ubicacion);
        Task<ApiResponse> EditarSeccionAsync(int idSeccion, string nombre, string ubicacion);
        Task<ApiResponse> EliminarSeccionAsync(int idSeccion);
        Task<PaginacionResponse<SeccionDto>> BuscarSeccionesRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
        Task<int> ObtenerIdSeccion(string nombre);
    }
}
