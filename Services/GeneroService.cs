using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class GeneroService : IGeneroService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeneroService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<GeneroDto>> ObtenerGenerosAsync(string? nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Genero/Lista-Generos?nombre={nombre}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var generos = await response.Content.ReadFromJsonAsync<List<GeneroDto>>();
                    return generos ?? new List<GeneroDto>();
                }
                return new List<GeneroDto>();
            }
            catch
            {
                return new List<GeneroDto>();
            }
        }
    }

    public interface IGeneroService
    {
        Task<List<GeneroDto>> ObtenerGenerosAsync(string? nombre);
    }
}
