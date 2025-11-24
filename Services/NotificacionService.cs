using AppWebBiblioteca.Models;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificacionService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
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

        public async Task<List<NotificacionView>> ObtenerNotificacionesAsync(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID de usuario proporcionado no es válido.", nameof(idUsuario));
            AgregarTokenAutenticacion();
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Notificacion/ObtenerNotificaciones?idUsuario={idUsuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error en la solicitud: {response.StatusCode}");

            // Deserializar directamente a List<NotificacionView>
            var notificaciones = await response.Content.ReadFromJsonAsync<List<NotificacionView>>();
            return notificaciones ?? new List<NotificacionView>();
        }

        public async Task<ApiResponse> MarcarComoLeidaAsync(int idNotificacion)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Notificacion/MarcarLeida?idNotificacion={idNotificacion}";
                var response = await _httpClient.PutAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
                    return apiResponse ?? new ApiResponse { Success = false, Message = "Respuesta inválida de la API" };
                }
                else
                {
                    // Si la respuesta no es exitosa, intentar leer el mensaje de error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ApiResponse errorResponse;

                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<ApiResponse>(errorContent, _jsonOptions)
                            ?? new ApiResponse { Success = false, Message = $"Error HTTP: {response.StatusCode}" };
                    }
                    catch
                    {
                        errorResponse = new ApiResponse { Success = false, Message = $"Error HTTP: {response.StatusCode}" };
                    }

                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al marcar notificación como leída: {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }


        public async Task<ApiResponse> EliminarTodasAsync(int idUsuario)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Notificacion/EliminarTodas?idUsuario={idUsuario}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
                    return apiResponse ?? new ApiResponse { Success = false, Message = "Respuesta inválida de la API" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ApiResponse errorResponse;
                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<ApiResponse>(errorContent, _jsonOptions)
                            ?? new ApiResponse { Success = false, Message = $"Error HTTP: {response.StatusCode}" };
                    }
                    catch
                    {
                        errorResponse = new ApiResponse { Success = false, Message = $"Error HTTP: {response.StatusCode}" };
                    }
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar todas las notificaciones: {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> MarcarTodasComoLeidasAsync(int idUsuario)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Notificacion/MarcarTodasLeidas?idUsuario={idUsuario}";
                var response = await _httpClient.PutAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
                    return apiResponse ?? new ApiResponse { Success = false, Message = "Respuesta inválida de la API" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ApiResponse errorResponse;

                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<ApiResponse>(errorContent, _jsonOptions)
                            ?? new ApiResponse { Success = false, Message = $"Error HTTP: {response.StatusCode}" };
                    }
                    catch
                    {
                        errorResponse = new ApiResponse { Success = false, Message = $"Error HTTP: {response.StatusCode}" };
                    }

                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al marcar todas las notificaciones como leídas: {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }


    }



    public interface INotificacionService
    {
        Task <List<NotificacionView>> ObtenerNotificacionesAsync(int idUsuario);
        Task<ApiResponse> MarcarComoLeidaAsync(int idNotificacion);
        Task<ApiResponse> MarcarTodasComoLeidasAsync(int idNotificacion);
        Task<ApiResponse> EliminarTodasAsync(int idUsuario);
    }

}

