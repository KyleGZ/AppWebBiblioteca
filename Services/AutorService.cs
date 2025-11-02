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

        // Crea un autor. Devuelve el Id generado por la API.
        public async Task<int> RegistrarAutorAsync(string nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Autor/Registro";
                var payload = new { IdAutor = 0, Nombre = nombre };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                if (!response.IsSuccessStatusCode) return 0;

                // La API responde: { mensaje, idAutor }
                var data = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                return data != null && data.TryGetValue("idUsuario", out var wrong) // por si alguna API usa otra key
                    ? Convert.ToInt32(wrong)
                    : (data != null && data.TryGetValue("idAutor", out var ok) ? Convert.ToInt32(ok) : 0);
            }
            catch
            {
                return 0;
            }
        }

        // Edita un autor existente (Id + Nombre). True si guardó.
        public async Task<bool> EditarAutorAsync(int idAutor, string nombre)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Autor/Editar";
                var payload = new { IdAutor = idAutor, Nombre = nombre };

                var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Elimina un autor por Id. True si eliminó.
        public async Task<bool> EliminarAutorAsync(int idAutor)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Autor/Eliminar?id={idAutor}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

    }

    public interface IAutorService
    {
        Task<List<AutorDto>> ObtenerAutoresAsync(string? nombre);
        Task<int> RegistrarAutorAsync(string nombre);
        Task<bool> EditarAutorAsync(int idAutor, string nombre);
        Task<bool> EliminarAutorAsync(int idAutor);
    }
}
