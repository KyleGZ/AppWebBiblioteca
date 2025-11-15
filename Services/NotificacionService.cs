using AppWebBiblioteca.Models;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public NotificacionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            

        }
        public async Task<ApiResponse> ObtenerNotificacionesAsync(int idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "El ID de usuario proporcionado no es válido.",
                        Data = null
                    };
                }

                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Notificacion/ObtenerNotificaciones?idUsuario={idUsuario}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return apiResponse ?? new ApiResponse { Success = false, Message = "Respuesta vacía de la API" };
                }

                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener notificaciones: {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }



    public interface INotificacionService
    {
        Task<ApiResponse> ObtenerNotificacionesAsync(int idUsuario);
    }

}

