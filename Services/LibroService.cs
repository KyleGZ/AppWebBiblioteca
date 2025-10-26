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
                
                if (string.IsNullOrWhiteSpace(termino))
                {
                    var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Libro/ListaView?pagina={pagina}&resultadosPorPagina={resultadosPorPagina}";
                    var response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultadoPaginado = await response.Content.ReadFromJsonAsync<PaginacionResponse<LibroListaView>>();

                        if (resultadoPaginado != null && resultadoPaginado.Success)
                        {
                            return resultadoPaginado;
                        }
                        else
                        {
                            return new PaginacionResponse<LibroListaView>
                            {
                                Success = false,
                                Message = "No se pudieron obtener los libros del catálogo"
                            };
                        }
                    }
                    else
                    {
                        return new PaginacionResponse<LibroListaView>
                        {
                            Success = false,
                            Message = $"Error al obtener el catálogo: {response.StatusCode}"
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

        /*
         * Metodo para editar libro
         */
        public async Task<ApiResponse> EditarLibroAsync(int idLibro, CrearLibroDto libroDto)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Libro/Editar-Libro?idLibro={idLibro}";

                var json = JsonSerializer.Serialize(libroDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
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
                    Message = $"Error al editar el libro: {ex.Message}"
                };
            }
        }


        /*
         * Este metodo obtiene la info del libro para actualizarlo
         */
        public async Task<ObtenerLibroEditar> ObtenerLibroParaEditarAsync(int idLibro)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Libro/ObtenerLibro-Editar?idLibro={idLibro}";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse.Success && apiResponse.Data != null)
                {
                    // Convertir el objeto Data a ObtenerLibroEditar
                    var jsonElement = (JsonElement)apiResponse.Data;
                    var libroEditar = JsonSerializer.Deserialize<ObtenerLibroEditar>(
                        jsonElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return libroEditar;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener libro para editar: {ex.Message}");
                return null;
            }
        }


        /*
         * Detalle del libro
         */

        public async Task<LibroDetalleDto> ObtenerDetalleLibroAsync(int idLibro)
        {

            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Libro/Detalle-Libro?idLibro={idLibro}";
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var libroDetalle = await response.Content.ReadFromJsonAsync<LibroDetalleDto>();
                    return libroDetalle ?? new LibroDetalleDto();
                }
                return new LibroDetalleDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener detalle del libro: {ex.Message}");
                return new LibroDetalleDto();
            }
        }

    }



    public interface ILibroService
    {
        Task<List<LibroListaView>> ObtenerLibrosAsync();
        Task<LibroDetalleDto> ObtenerDetalleLibroAsync(int idLibro);
        Task<PaginacionResponse<LibroListaView>> BuscarLibrosRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
        Task<PaginacionResponse<LibroListaView>> BuscarLibrosDescripcionAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
        Task<ApiResponse> RegistrarLibroAsync(CrearLibroDto crearLibroDto);
        Task<ObtenerLibroEditar> ObtenerLibroParaEditarAsync(int idLibro);
        Task<ApiResponse> EditarLibroAsync(int idLibro, CrearLibroDto libroDto);

    }
}
