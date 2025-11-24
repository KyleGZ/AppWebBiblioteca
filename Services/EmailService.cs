using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public EmailService(HttpClient httpClient, IConfiguration configuration, ILogger<EmailService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
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

        public async Task<EmailSettings> GetSettingsAsync()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/EmailSettings/GetSettings";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch email settings. Status Code: {StatusCode}", response.StatusCode);
                    return new EmailSettings();
                }

                var emailSettings = await response.Content.ReadFromJsonAsync<EmailSettings>();
                return emailSettings ?? new EmailSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching email settings");
                return new EmailSettings();
            }

        }
        public async Task<ApiResponse> UpdateSettingsAsync(UpdateEmailSettings settings)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/EmailSettings/UpdateSettings";
                var response = await _httpClient.PutAsJsonAsync(apiUrl, settings);

                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta exitosa
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return apiResponse ?? new ApiResponse
                    {
                        Success = true,
                        Message = "Configuración actualizada correctamente"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Leer la respuesta de bad request
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return apiResponse ?? new ApiResponse
                    {
                        Success = false,
                        Message = "Error de validación en la solicitud"
                    };
                }
                else
                {
                    // Para otros códigos de error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("No se pudo actualizar la configuración. Código: {StatusCode}, Respuesta: {Error}",
                        response.StatusCode, errorContent);

                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error de conexión al actualizar la configuración de correo.");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Error de conexión con el servidor"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar la configuración de correo.");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Error inesperado al procesar la solicitud"
                };
            }
        }
        public async Task<ApiResponse> TestConnectionAsync()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/EmailSettings/TestConnection";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta exitosa de la API
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return apiResponse ?? new ApiResponse
                    {
                        Success = true,
                        Message = "Conexión SMTP probada exitosamente"
                    };
                }
                else
                {
                    // Leer la respuesta de error de la API
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

                    if (apiResponse != null)
                    {
                        return apiResponse;
                    }

                    // Fallback si no se pudo leer la respuesta
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error de conexión al probar configuración SMTP.");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Error de conexión con el servidor API"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al probar conexión SMTP.");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Error inesperado al procesar la solicitud"
                };
            }
        }
    }

    public interface IEmailService
    {
        Task<EmailSettings> GetSettingsAsync();
        Task<ApiResponse> UpdateSettingsAsync(UpdateEmailSettings settings);
        Task<ApiResponse> TestConnectionAsync();

    }
}
