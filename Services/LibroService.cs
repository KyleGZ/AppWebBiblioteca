using AppWebBiblioteca.Models;
using System.Text;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class LibroService : ILibroService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public LibroService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<LibroListaView>> ObtenerLibrosAsync()
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Libro/ListaView";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var libros = await response.Content.ReadFromJsonAsync<List<LibroListaView>>();
                    return libros ?? new List<LibroListaView>();
                }

                return new List<LibroListaView>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener libros: {ex.Message}");
                return new List<LibroListaView>();
            }
        }

        public async Task<PaginacionResponse<LibroListaView>> BuscarLibrosRapidaAsync(
       string termino,
       int pagina = 1,
       int resultadosPorPagina = 20)
        {
            try
            {
                // Si el término está vacío, usar el endpoint que devuelve todos los libros
                if (string.IsNullOrWhiteSpace(termino))
                {
                    var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Libro/ListaView";
                    var response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var todosLosLibros = await response.Content.ReadFromJsonAsync<List<LibroListaView>>();

                        // Paginar manualmente los resultados
                        var librosPaginados = todosLosLibros?
                            .Skip((pagina - 1) * resultadosPorPagina)
                            .Take(resultadosPorPagina)
                            .ToList() ?? new List<LibroListaView>();

                        return new PaginacionResponse<LibroListaView>
                        {
                            Success = true,
                            Message = $"Se encontraron {todosLosLibros?.Count ?? 0} libros en el catálogo",
                            Data = librosPaginados,
                            Pagination = new PaginationInfo
                            {
                                PaginaActual = pagina,
                                TotalPaginas = (int)Math.Ceiling((todosLosLibros?.Count ?? 0) / (double)resultadosPorPagina),
                                TotalResultados = todosLosLibros?.Count ?? 0,
                                ResultadosPorPagina = resultadosPorPagina
                            }
                        };
                    }
                }




                var buscarUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Libro/buscar-rapida?termino={Uri.EscapeDataString(termino)}&pagina={pagina}&resultadosPorPagina={resultadosPorPagina}";

                var buscarResponse = await _httpClient.GetAsync(buscarUrl);

                if (buscarResponse.IsSuccessStatusCode)
                {
                    var result = await buscarResponse.Content.ReadFromJsonAsync<PaginacionResponse<LibroListaView>>();
                    return result ?? new PaginacionResponse<LibroListaView>
                    {
                        Success = false,
                        Message = "No se pudieron procesar los resultados"
                    };
                }
                else
                {
                    return new PaginacionResponse<LibroListaView>
                    {
                        Success = false,
                        Message = $"Error en la búsqueda: {buscarResponse.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<LibroListaView>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }

        /*
         * Busqueda por descripcion
         */

        public async Task<PaginacionResponse<LibroListaView>> BuscarLibrosDescripcionAsync(
       string termino,
       int pagina = 1,
       int resultadosPorPagina = 20)
        {
            try
            {
                var buscarUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Libro/Busqueda-Descripcion?terminoBusqueda={Uri.EscapeDataString(termino)}&pagina={pagina}&resultadosPorPagina={resultadosPorPagina}";

                var buscarResponse = await _httpClient.GetAsync(buscarUrl);

                if (buscarResponse.IsSuccessStatusCode)
                {
                    var result = await buscarResponse.Content.ReadFromJsonAsync<PaginacionResponse<LibroListaView>>();
                    return result ?? new PaginacionResponse<LibroListaView>
                    {
                        Success = false,
                        Message = "No se pudieron procesar los resultados"
                    };
                }
                else
                {
                    return new PaginacionResponse<LibroListaView>
                    {
                        Success = false,
                        Message = $"Error en la búsqueda por descripción: {buscarResponse.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<LibroListaView>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> RegistrarLibroAsync(CrearLibroDto libroDto)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Libro/Registro-Libro";

                var json = JsonSerializer.Serialize(libroDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse ?? new ApiResponse
                {
                    Success = false,
                    Message = "No se pudo procesar la respuesta del servidor"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error al registrar el libro: {ex.Message}"
                };
            }
        }


    }


    
    public interface ILibroService
    {
        Task<List<LibroListaView>> ObtenerLibrosAsync();
        Task<PaginacionResponse<LibroListaView>> BuscarLibrosRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
        Task<PaginacionResponse<LibroListaView>> BuscarLibrosDescripcionAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
        Task<ApiResponse> RegistrarLibroAsync(CrearLibroDto crearLibroDto);

    }
}
