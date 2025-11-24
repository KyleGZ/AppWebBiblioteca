using AppWebBiblioteca.Models;
using System.Text;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class LibroService : ILibroService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LibroService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
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

        public async Task<List<LibroListaView>> ObtenerLibrosAsync()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Libro/ListaView";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Intenta deserializar como lista directa primero
                    try
                    {
                        var librosDirectos = await response.Content.ReadFromJsonAsync<List<LibroListaView>>();
                        if (librosDirectos != null && librosDirectos.Any())
                        {
                            return librosDirectos;
                        }
                    }
                    catch (JsonException ex)
                    {

                    }

                    // Si falla, intenta deserializar como ApiResponse
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (apiResponse?.Success == true && apiResponse.Data != null)
                        {
                            var jsonElement = (JsonElement)apiResponse.Data;
                            var librosFromApiResponse = JsonSerializer.Deserialize<List<LibroListaView>>(
                                jsonElement.GetRawText(),
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            return librosFromApiResponse ?? new List<LibroListaView>();
                        }
                    }
                    catch (JsonException ex)
                    {
                    }

                    // Si ambos fallan, intenta deserializar como PaginacionResponse
                    try
                    {
                        var paginatedResponse = JsonSerializer.Deserialize<PaginacionResponse<LibroListaView>>(responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (paginatedResponse?.Success == true && paginatedResponse.Data != null)
                        {
                            return paginatedResponse.Data;
                        }
                    }
                    catch (JsonException ex)
                    {
                    }
                }
                else
                {

                }

                return new List<LibroListaView>();
            }
            catch (Exception ex)
            {
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
                    AgregarTokenAutenticacion();
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


                AgregarTokenAutenticacion();
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
                AgregarTokenAutenticacion();

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
                AgregarTokenAutenticacion();

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
                AgregarTokenAutenticacion();

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
                AgregarTokenAutenticacion();

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
                AgregarTokenAutenticacion();

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
                return new LibroDetalleDto();
            }
        }

        /*
         * 
         */

        public async Task<byte[]> DescargarPlantillaImportacionAsync()
        {
            try
            {
                AgregarTokenAutenticacion();

                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Libro/Plantilla-Importacion";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                throw new Exception($"Error al descargar plantilla: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ApiResponse> ImportarLibrosDesdeExcelAsync(IFormFile archivo)
        {
            try
            {
                AgregarTokenAutenticacion();

                var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Libro/Importar-Libro";

                using var content = new MultipartFormDataContent();
                using var fileStream = archivo.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);

                content.Add(fileContent, "archivo", archivo.FileName);

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Procesar respuesta basada en el código de estado
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                if (response.IsSuccessStatusCode)
                {
                    // Éxito (200)
                    return new ApiResponse
                    {
                        Success = true,
                        Message = root.GetProperty("mensaje").GetString(),
                        Data = root.GetProperty("datos").Deserialize<object>()
                    };
                }
                else
                {
                    // Error (400, 500, etc.)
                    string mensaje = root.TryGetProperty("mensaje", out var mensajeProp)
                        ? mensajeProp.GetString()
                        : $"Error del servidor ({(int)response.StatusCode})";

                    object datos = root.TryGetProperty("datos", out var datosProp)
                        ? datosProp.Deserialize<object>()
                        : root.TryGetProperty("error", out var errorProp)
                            ? new { error = errorProp.GetString() }
                            : new { codigo = (int)response.StatusCode, detalle = responseContent };

                    return new ApiResponse
                    {
                        Success = false,
                        Message = mensaje,
                        Data = datos
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error durante la importación: {ex.Message}. Se deshicieron todos los cambios.",
                    Data = new
                    {
                        innerException = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace,
                        insertados = 0
                    }
                };
            }
        }

        public async Task<int> ObtenerIdLibro(string isbn)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Libro/Get-libro?isbn={Uri.EscapeDataString(isbn)}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return 0;

                // Leer contenido como string
                var content = await response.Content.ReadAsStringAsync();

                // Intentar convertir directamente a int
                if (int.TryParse(content, out int idLibro))
                    return idLibro;

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
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

        Task<byte[]> DescargarPlantillaImportacionAsync();
        Task<ApiResponse> ImportarLibrosDesdeExcelAsync(IFormFile archivo);
        Task<int> ObtenerIdLibro(string isbn);

    }
}
