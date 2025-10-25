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
    }

    public interface ISeccionService
    {
        Task<List<SeccionDto>> ObtenerSeccionesAsync(string? nombre);
    }
}
