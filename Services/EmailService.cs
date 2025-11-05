using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        public EmailService(HttpClient httpClient, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<EmailSettings> GetSettingsAsync()
        {
            try
            {
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
             public async Task<bool> UpdateSettingsAsync(EmailSettings settings)
            {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/EmailSettings/UpdateSettings";
                var response = await _httpClient.PutAsJsonAsync(apiUrl, settings);

                if (response.IsSuccessStatusCode)
                    return true;

                _logger.LogWarning("No se pudo actualizar la configuración. Código: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la configuración de correo.");
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/EmailSettings/TestConnection";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                    return true;

                _logger.LogWarning("Prueba de conexión SMTP fallida. Código: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar la conexión SMTP.");
                return false;
            }
        }
    }

    public interface IEmailService
    {
        Task<EmailSettings> GetSettingsAsync();
        Task<bool> UpdateSettingsAsync(EmailSettings settings);
        Task<bool> TestConnectionAsync();

    }
}
