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

        public async Task<PaginacionResponse<AutorDto>> BuscarAutoresRapidaAsync(
         string termino,
         int pagina = 1,
          int resultadosPorPagina = 20)
        {
            try
            {
                // Si no hay término => listar paginado
                if (string.IsNullOrWhiteSpace(termino))
                {
                    var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Autor/ListarViewAutor?pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                    var response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultado = await response.Content.ReadFromJsonAsync<PaginacionResponse<AutorDto>>();
                        if (resultado != null && resultado.Success)
                        {
                            return resultado;
                        }
                        return new PaginacionResponse<AutorDto>
                        {
                            Success = false,
                            Message = "No se pudieron obtener los autores"
                        };
                    }

                    return new PaginacionResponse<AutorDto>
                    {
                        Success = false,
                        Message = $"Error al obtener los autores: {response.StatusCode}"
                    };
                }

                // Con término => búsqueda paginada
                var buscarUrl =
                    $"{_configuration["ApiSettings:BaseUrl"]}/Autor/Busqueda-Autor" +
                    $"?termino={Uri.EscapeDataString(termino.Trim())}" +
                    $"&pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";

                var buscarResponse = await _httpClient.GetAsync(buscarUrl);

                if (buscarResponse.IsSuccessStatusCode)
                {
                    var result = await buscarResponse.Content.ReadFromJsonAsync<PaginacionResponse<AutorDto>>();
                    return result ?? new PaginacionResponse<AutorDto>
                    {
                        Success = false,
                        Message = "No se pudieron procesar los resultados de la búsqueda"
                    };
                }

                return new PaginacionResponse<AutorDto>
                {
                    Success = false,
                    Message = $"Error en la búsqueda: {buscarResponse.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<AutorDto>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
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
        Task<PaginacionResponse<AutorDto>> BuscarAutoresRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);

        Task<int> RegistrarAutorAsync(string nombre);
        Task<bool> EditarAutorAsync(int idAutor, string nombre);
        Task<bool> EliminarAutorAsync(int idAutor);
    }
}
