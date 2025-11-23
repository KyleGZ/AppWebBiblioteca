using System.Text.Json;
using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public class GeneroService : IGeneroService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GeneroService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        // ✅ Registrar un nuevo género
        //public async Task<int> RegistrarGeneroAsync(string nombre)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Genero/Registro";
        //        var payload = new { IdGenero = 0, Nombre = nombre };

        //        var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
        //        if (!response.IsSuccessStatusCode) return 0;

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        if (root.TryGetProperty("success", out var okProp) && okProp.GetBoolean() &&
        //            root.TryGetProperty("data", out var dataProp) &&
        //            dataProp.ValueKind == JsonValueKind.Object &&
        //            dataProp.TryGetProperty("idGenero", out var idProp) &&
        //            idProp.TryGetInt32(out var id))
        //        {
        //            return id;
        //        }

        //        return 0;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        // ✅ Registrar un nuevo género con ApiResponse

        public async Task<ApiResponse> RegistrarGeneroAsync(string nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Genero/Registro";
                var payload = new { IdGenero = 0, Nombre = nombre };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                var json = await response.Content.ReadAsStringAsync();

                // Log para debugging
                Console.WriteLine($"API Response: {json}");

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, json);
                }

                // Procesar respuesta exitosa
                try
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse ?? new ApiResponse
                    {
                        Success = false,
                        Message = "No se pudo procesar la respuesta del servidor"
                    };
                }
                catch (JsonException)
                {
                    // Si la deserialización falla, intentar extraer manualmente
                    return ExtractFromJson(json);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }


        //public async Task<ApiResponse> RegistrarGeneroAsync(string nombre)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Genero/Registro";
        //        var payload = new { IdGenero = 0, Nombre = nombre };

        //        var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
        //        if (!response.IsSuccessStatusCode)
        //            return new ApiResponse { Success = false, Message = "Error en la comunicación con el API" };

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        if (root.TryGetProperty("success", out var okProp) && okProp.GetBoolean())
        //        {
        //            string message = "Registro exitoso";
        //            if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
        //            {
        //                message = messageProp.GetString();
        //            }

        //            object data = null;
        //            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
        //            {
        //                data = JsonSerializer.Deserialize<object>(dataProp.GetRawText());
        //            }

        //            return new ApiResponse { Success = true, Message = message, Data = data };
        //        }
        //        else
        //        {
        //            string errorMessage = "Error en el registro";
        //            if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
        //            {
        //                errorMessage = messageProp.GetString();
        //            }

        //            return new ApiResponse { Success = false, Message = errorMessage };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse { Success = false, Message = $"Error interno: {ex.Message}" };
        //    }
        //}



        // ✅ Editar género existente
        //public async Task<bool> EditarGeneroAsync(int idGenero, string nombre)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Genero/Editar";
        //        var payload = new { IdGenero = idGenero, Nombre = nombre };

        //        var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);
        //        if (!response.IsSuccessStatusCode) return false;

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        return root.TryGetProperty("success", out var okProp) && okProp.GetBoolean();
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


        public async Task<ApiResponse> EditarGeneroAsync(int idGenero, string nombre)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Genero/Editar";
                var payload = new { IdGenero = idGenero, Nombre = nombre };

                var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);
                var json = await response.Content.ReadAsStringAsync();

                // Debug opcional
                // Console.WriteLine($"API Edit Response - Status: {response.StatusCode}, Body: {json}");

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, json);
                }

                // Procesar respuesta exitosa usando ExtractFromJson
                return ExtractFromJson(json);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }

        //public async Task<ApiResponse> EditarGeneroAsync(int idGenero, string nombre)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Genero/Editar";
        //        var payload = new { IdGenero = idGenero, Nombre = nombre };

        //        var response = await _httpClient.PutAsJsonAsync(apiUrl, payload);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return new ApiResponse
        //            {
        //                Success = false,
        //                Message = $"Error HTTP: {response.StatusCode}"
        //            };
        //        }

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        var apiResponse = new ApiResponse
        //        {
        //            Success = root.TryGetProperty("success", out var okProp) && okProp.GetBoolean(),
        //            Message = root.TryGetProperty("message", out var msgProp)
        //                     ? msgProp.GetString()
        //                     : "Operación completada"
        //        };

        //        // Opcional: incluir data si existe
        //        if (root.TryGetProperty("data", out var dataProp))
        //        {
        //            apiResponse.Data = dataProp.GetRawText();
        //        }

        //        return apiResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse
        //        {
        //            Success = false,
        //            Message = $"Error inesperado: {ex.Message}"
        //        };
        //    }
        //}

        // ✅ Eliminar género
        //public async Task<bool> EliminarGeneroAsync(int idGenero)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Genero/Eliminar?id={idGenero}";
        //        var response = await _httpClient.DeleteAsync(apiUrl);
        //        if (!response.IsSuccessStatusCode) return false;

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        return root.TryGetProperty("success", out var okProp) && okProp.GetBoolean();
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        public async Task<ApiResponse> EliminarGeneroAsync(int idGenero)
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Genero/Eliminar?id={idGenero}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                var json = await response.Content.ReadAsStringAsync();

                // Debug opcional
                // Console.WriteLine($"API Delete Response - Status: {response.StatusCode}, Body: {json}");

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, json);
                }

                // Procesar respuesta exitosa usando ExtractFromJson
                return ExtractFromJson(json);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }

        //public async Task<ApiResponse> EliminarGeneroAsync(int idGenero)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Genero/Eliminar?id={idGenero}";
        //        var response = await _httpClient.DeleteAsync(apiUrl);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return new ApiResponse
        //            {
        //                Success = false,
        //                Message = $"Error HTTP: {response.StatusCode}"
        //            };
        //        }

        //        var json = await response.Content.ReadAsStringAsync();
        //        using var doc = JsonDocument.Parse(json);
        //        var root = doc.RootElement;

        //        var apiResponse = new ApiResponse
        //        {
        //            Success = root.TryGetProperty("success", out var okProp) && okProp.GetBoolean(),
        //            Message = root.TryGetProperty("message", out var msgProp)
        //                     ? msgProp.GetString()
        //                     : "Operación completada"
        //        };

        //        // Opcional: incluir data si existe
        //        if (root.TryGetProperty("data", out var dataProp))
        //        {
        //            apiResponse.Data = dataProp.GetRawText();
        //        }

        //        return apiResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse
        //        {
        //            Success = false,
        //            Message = $"Error inesperado: {ex.Message}"
        //        };
        //    }
        //}

        // ✅ Búsqueda y paginación
        public async Task<PaginacionResponse<GeneroDto>> BuscarGenerosRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20)
        {
            try
            {
                string apiUrl;
                if (string.IsNullOrWhiteSpace(termino))
                {
                    apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Genero/ListarViewGenero?pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                }
                else
                {
                    apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Genero/Busqueda-Genero?termino={Uri.EscapeDataString(termino)}&pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                }

                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return new PaginacionResponse<GeneroDto>
                    {
                        Success = false,
                        Message = $"Error al obtener los géneros: {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!(root.TryGetProperty("success", out var okProp) && okProp.GetBoolean()))
                {
                    var msg = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Error en la respuesta";
                    return new PaginacionResponse<GeneroDto> { Success = false, Message = msg ?? "Error en la respuesta" };
                }

                if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                {
                    var paginado = dataProp.Deserialize<PaginacionResponse<GeneroDto>>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (paginado != null)
                    {
                        paginado.Success = true;
                        return paginado;
                    }
                }

                return new PaginacionResponse<GeneroDto>
                {
                    Success = false,
                    Message = "No se pudieron procesar los resultados"
                };
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<GeneroDto>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }


        private async Task<ApiResponse> HandleErrorResponse(HttpResponseMessage response, string json)
        {
            try
            {
                // Intentar deserializar como ApiResponse
                var errorResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    return errorResponse;
                }

                // Fallback: extraer mensaje manualmente
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var errorMessage = root.TryGetProperty("message", out var msg)
                    ? msg.GetString()
                    : root.TryGetProperty("title", out var title)
                        ? title.GetString()
                        : $"Error del servidor (HTTP {(int)response.StatusCode})";

                return new ApiResponse { Success = false, Message = errorMessage };
            }
            catch
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error en la comunicación con el API (HTTP {(int)response.StatusCode})"
                };
            }
        }

        private ApiResponse ExtractFromJson(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                bool success = root.TryGetProperty("success", out var s) && s.GetBoolean();
                string message = root.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : (success ? "Operación exitosa" : "Error en la operación");

                object data = null;
                if (root.TryGetProperty("data", out var d) && d.ValueKind != JsonValueKind.Null)
                {
                    data = JsonSerializer.Deserialize<object>(d.GetRawText());
                }

                return new ApiResponse { Success = success, Message = message, Data = data };
            }
            catch
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Formato de respuesta inválido"
                };
            }
        }


    }

    public interface IGeneroService
    {
        Task<List<GeneroDto>> ObtenerGenerosAsync(string? nombre);
        Task<ApiResponse> RegistrarGeneroAsync(string nombre);
        Task<ApiResponse> EditarGeneroAsync(int idGenero, string nombre);
        Task<ApiResponse> EliminarGeneroAsync(int idGenero);
        Task<PaginacionResponse<GeneroDto>> BuscarGenerosRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
    }
}
