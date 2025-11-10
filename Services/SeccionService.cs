using System.Text.Json;
using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class SeccionService : ISeccionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SeccionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<SeccionDto>> ObtenerSeccionesAsync(string? nombre)
        {
            try
            {
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

        // Registrar una nueva sección
        public async Task<ApiResponse> RegistrarSeccionAsync(string nombre, string ubicacion)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Seccion/Registro";
                var payload = new { IdSeccion = 0, Nombre = nombre, Ubicacion = ubicacion };

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



        //public async Task<int> RegistrarSeccionAsync(string nombre, string ubicacion)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Seccion/Registro";
        //        var payload = new { IdSeccion = 0, Nombre = nombre, Ubicacion = ubicacion };

        //        var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
        //        if (!response.IsSuccessStatusCode) return 0;

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        if (root.TryGetProperty("success", out var okProp) && okProp.GetBoolean() &&
        //            root.TryGetProperty("data", out var dataProp) &&
        //            dataProp.ValueKind == JsonValueKind.Object &&
        //            dataProp.TryGetProperty("idSeccion", out var idProp) &&
        //            idProp.TryGetInt32(out var id))
        //        {
        //            return id;
        //        }

        //        return 0;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        // Editar sección existente
        //public async Task<bool> EditarSeccionAsync(int idSeccion, string nombre, string ubicacion)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Seccion/Editar";
        //        var payload = new { IdSeccion = idSeccion, Nombre = nombre, Ubicacion = ubicacion };

        //        var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);
        //        if (!response.IsSuccessStatusCode) return false;

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        return root.TryGetProperty("success", out var okProp) && okProp.GetBoolean();
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public async Task<ApiResponse> EditarSeccionAsync(int idSeccion, string nombre, string ubicacion)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Seccion/Editar";
                var payload = new { IdSeccion = idSeccion, Nombre = nombre, Ubicacion = ubicacion };

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

        // Eliminar sección
        //public async Task<bool> EliminarSeccionAsync(int idSeccion)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Seccion/Eliminar?id={idSeccion}";
        //        var response = await _httpClient.DeleteAsync(apiUrl);
        //        if (!response.IsSuccessStatusCode) return false;

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        return root.TryGetProperty("success", out var okProp) && okProp.GetBoolean();
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public async Task<ApiResponse> EliminarSeccionAsync(int idSeccion)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Seccion/Eliminar?id={idSeccion}";
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

                // Asumiendo que la API ya retorna una estructura similar
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

        // Búsqueda y paginación
        public async Task<PaginacionResponse<SeccionDto>> BuscarSeccionesRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20)
        {
            try
            {
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
    }

    public interface ISeccionService
    {
        Task<List<SeccionDto>> ObtenerSeccionesAsync(string? nombre);
        Task<ApiResponse> RegistrarSeccionAsync(string nombre, string ubicacion);
        Task<ApiResponse> EditarSeccionAsync(int idSeccion, string nombre, string ubicacion);
        Task<ApiResponse> EliminarSeccionAsync(int idSeccion);
        Task<PaginacionResponse<SeccionDto>> BuscarSeccionesRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
    }
}
