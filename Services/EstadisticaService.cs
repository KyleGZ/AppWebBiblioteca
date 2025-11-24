using System.Text.Json;
using System.Text;
using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{

    public class EstadisticaService: IEstadisticaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public EstadisticaService(IConfiguration configuration, HttpClient cliente, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClient = cliente;
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

        //public async Task<EstadisticasPrestamosDTO> ObtenerEstadisticasAsync()
        //{

        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"]+ "/api/Prestamos/GetEstadisticasPrestamos";
        //        var response = await _httpClient.GetAsync(apiUrl);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var estadisticas = await response.Content.ReadFromJsonAsync<EstadisticasPrestamosDTO>();
        //            return estadisticas ?? new EstadisticasPrestamosDTO();
        //        }
        //        else
        //        {
        //            _logger.LogWarning("Error al obtener estadísticas. Código: {StatusCode}", response.StatusCode);
        //            return new EstadisticasPrestamosDTO();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al consumir la API de estadísticas");
        //        return new EstadisticasPrestamosDTO();
        //    }
        //}

        public async Task<EstadisticasPrestamosDTO> ObtenerEstadisticasAsync()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos/GetEstadisticasPrestamos";
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

        public async Task<EstadisticasPrestamosDTO> ObtenerEstadisticasPorFiltroAsync(FiltroEstadisticasDTO filtro)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(filtro),
                    Encoding.UTF8,
                    "application/json");
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos/GetEstadisticasPorFiltro";
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var estadisticas = await response.Content.ReadFromJsonAsync<EstadisticasPrestamosDTO>();
                    return estadisticas ?? new EstadisticasPrestamosDTO();
                }
                else
                {
                    _logger.LogWarning("Error al obtener estadísticas filtradas. Código: {StatusCode}", response.StatusCode);
                    return new EstadisticasPrestamosDTO();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la API de estadísticas filtradas");
                return new EstadisticasPrestamosDTO();
            }
        }

        public async Task<byte[]> DescargarReporteExcelAsync(FiltroEstadisticasDTO filtro)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(filtro),
                    Encoding.UTF8,
                    "application/json");
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Prestamos/DescargarReporteExcel";
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    _logger.LogWarning("Error al descargar reporte Excel. Código: {StatusCode}", response.StatusCode);
                    throw new Exception("Error al generar el reporte Excel");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la API de reportes Excel");
                throw;
            }
        }

    }

    
    public interface IEstadisticaService
    {
        Task<EstadisticasPrestamosDTO> ObtenerEstadisticasAsync();
        Task<EstadisticasPrestamosDTO> ObtenerEstadisticasPorFiltroAsync(FiltroEstadisticasDTO filtro);
        Task<byte[]> DescargarReporteExcelAsync(FiltroEstadisticasDTO filtro);
    }
}

