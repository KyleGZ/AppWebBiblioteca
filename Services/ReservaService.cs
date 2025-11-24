using AppWebBiblioteca.Models;
using System.Text;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class ReservaService : IReservaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ReservaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PaginacionResponse<ReservaResponseDto>> ObtenerReservasAsync(
        string termino = "",
        int pagina = 1,
        int resultadosPorPagina = 10,
        int? userId = null,
        string estadoFiltro = "Activa",
        bool esAdministrador = false)
        {
            try
            {
                var url = _configuration["ApiSettings:BaseUrl"] + "/Reserva/ListaReservas";

                var parameters = new List<string>();
                // SOLO enviar userId si NO es administrador
                if (!esAdministrador && userId.HasValue)
                    parameters.Add($"userId={userId.Value}");

                if (!string.IsNullOrEmpty(estadoFiltro))
                    parameters.Add($"estado={estadoFiltro}");

                parameters.Add($"esAdministrador={esAdministrador}");

                if (parameters.Any())
                    url += "?" + string.Join("&", parameters);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var reservas = await response.Content.ReadFromJsonAsync<List<ReservaResponseDto>>();

                    // Implementar paginación manualmente
                    var reservasFiltradas = FiltrarReservas(reservas ?? new List<ReservaResponseDto>(), termino);
                    var totalResultados = reservasFiltradas.Count;
                    var totalPaginas = (int)Math.Ceiling((double)totalResultados / resultadosPorPagina);

                    var reservasPaginadas = reservasFiltradas
                        .Skip((pagina - 1) * resultadosPorPagina)
                        .Take(resultadosPorPagina)
                        .ToList();

                    return new PaginacionResponse<ReservaResponseDto>
                    {
                        Success = true,
                        Data = reservasPaginadas,
                        Pagination = new PaginationInfo
                        {
                            PaginaActual = pagina,
                            ResultadosPorPagina = resultadosPorPagina,
                            TotalResultados = totalResultados,
                            TotalPaginas = totalPaginas
                        }
                    };
                }

                return new PaginacionResponse<ReservaResponseDto>
                {
                    Success = false,
                    Message = "Error al obtener reservas",
                    Data = new List<ReservaResponseDto>(),
                    Pagination = new PaginationInfo()
                };
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<ReservaResponseDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = new List<ReservaResponseDto>(),
                    Pagination = new PaginationInfo()
                };
            }
        }

        // MÉTODO PARA OBTENER SOLO RESERVAS ACTIVAS
        public async Task<PaginacionResponse<ReservaResponseDto>> ObtenerReservasActivasAsync(
            string termino = "",
            int pagina = 1,
            int resultadosPorPagina = 10,
            int? userId = null, bool esAdministrador = false)
        {
            return await ObtenerReservasAsync(termino, pagina, resultadosPorPagina, userId, "Activa", esAdministrador);
        }

        // MÉTODO PARA OBTENER RESERVAS HISTÓRICAS (NO ACTIVAS)
        public async Task<PaginacionResponse<ReservaResponseDto>> ObtenerReservasHistoricasAsync(
            string termino = "",
            int pagina = 1,
            int resultadosPorPagina = 10,
            int? userId = null, bool esAdministrador = false)
        {
            return await ObtenerReservasAsync(termino, pagina, resultadosPorPagina, userId, "NoActiva", esAdministrador);
        }

        // Obtener conteo de reservas activas del usuario
        public async Task<int> ObtenerConteoReservasActivasAsync(int userId)
        {
            try
            {
                var url = _configuration["ApiSettings:BaseUrl"] + $"/Reserva/ConteoReservasActivas?userId={userId}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    if (result.Success && result.Data is JsonElement jsonElement)
                    {
                        return jsonElement.GetInt32();
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener conteo de reservas activas: {ex.Message}");
                return 0;
            }
        }

        // FILTRAR RESERVAS
        private List<ReservaResponseDto> FiltrarReservas(List<ReservaResponseDto> reservas, string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return reservas;

            return reservas.Where(r =>
                (r.NombreUsuario?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.TituloLibro?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.Estado?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false) ||
                r.IdReserva.ToString().Contains(termino)
            ).ToList();
        }

        // OBTENER RESERVA POR ID
        public async Task<ReservaResponseDto> ObtenerReservaIDAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Reserva/BuscarReservaID?id={id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ReservaResponseDto>();
                    return result;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // REGISTRAR RESERVA
        public async Task<ApiResponse> RegistrarReservaAsync(ReservaDto reservaDto)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Reserva/CrearReserva";

                var json = JsonSerializer.Serialize(reservaDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse ?? new ApiResponse
                {
                    Success = false,
                    Message = "No se pudo procesar la respuesta del servidor"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error al registrar la reserva: {ex.Message}"
                };
            }
        }

        // ELIMINAR RESERVA
        public async Task<ApiResponse> EliminarReservaAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Reserva/EliminarReserva?id={id}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, json);
                }

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

        // MANEJO DE RESPUESTAS DE ERROR
        private async Task<ApiResponse> HandleErrorResponse(HttpResponseMessage response, string json)
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    return errorResponse;
                }

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

        // EXTRAER DATOS DE JSON
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
    }

    // INTERFAZ IReservaService
    public interface IReservaService
    {
        Task<PaginacionResponse<ReservaResponseDto>> ObtenerReservasAsync(string termino = "", int pagina = 1, int resultadosPorPagina = 10, int? userId = null, string estadoFiltro = "Activa", bool esAdministrador = false);
        Task<PaginacionResponse<ReservaResponseDto>> ObtenerReservasActivasAsync(string termino = "", int pagina = 1, int resultadosPorPagina = 10, int? userId = null, bool esAdministrador = false);
        Task<PaginacionResponse<ReservaResponseDto>> ObtenerReservasHistoricasAsync(string termino = "", int pagina = 1, int resultadosPorPagina = 10, int? userId = null, bool esAdministrador = false);
        Task<ReservaResponseDto> ObtenerReservaIDAsync(int id);
        Task<ApiResponse> RegistrarReservaAsync(ReservaDto reservaDto);
        Task<ApiResponse> EliminarReservaAsync(int id);
        Task<int> ObtenerConteoReservasActivasAsync(int userId);
    }
}//