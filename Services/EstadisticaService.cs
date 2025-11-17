using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{

    public class EstadisticaService: IEstadisticaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public EstadisticaService(IConfiguration configuration, HttpClient cliente)
        {
            _configuration = configuration;
            _httpClient = cliente;
        }
        public async Task<EstadisticasPrestamosDTO> ObtenerEstadisticasAsync()
        {

            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"]+ "/api/Prestamos/GetEstadisticasPrestamos";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var estadisticas = await response.Content.ReadFromJsonAsync<EstadisticasPrestamosDTO>();
                    return estadisticas ?? new EstadisticasPrestamosDTO();
                }
                else
                {
                    _logger.LogWarning("Error al obtener estadísticas. Código: {StatusCode}", response.StatusCode);
                    return new EstadisticasPrestamosDTO();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la API de estadísticas");
                return new EstadisticasPrestamosDTO();
            }
        }

    }

    
    public interface IEstadisticaService
    {
        Task<EstadisticasPrestamosDTO> ObtenerEstadisticasAsync();
    }
}

