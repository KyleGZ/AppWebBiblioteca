using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class AutorService : IAutorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AutorService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<AutorDto>> ObtenerAutoresAsync(string? nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Autor/Lista-Autores?nombre={nombre}";
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var autores = await response.Content.ReadFromJsonAsync<List<AutorDto>>();
                    return autores ?? new List<AutorDto>();
                }
                return new List<AutorDto>();
            }
            catch
            {
                return new List<AutorDto>();
            }
        }

    }

    public interface IAutorService
    {
        Task<List<AutorDto>> ObtenerAutoresAsync(string? nombre);
    }
}
