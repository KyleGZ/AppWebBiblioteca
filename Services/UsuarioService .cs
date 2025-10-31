using AppWebBiblioteca.Models;
using System.Text;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{

    public interface IUsuarioService
    {
        Task<List<UsuarioListaViewModel>> ObtenerUsuariosAsync();
        Task<PaginacionResponse<UsuarioListaViewModel>> BuscarUsuariosRapidaAsync(string termino, int pagina = 1, int resultadosPorPagina = 20);
        Task<List<string>> ObtenerRolesDeUsuarioAsync(int idUsuario);
        Task<UsuarioListaViewModel> ObtenerUsuarioPorIdAsync(int id);
        Task<PerfilUsuarioDto> PerfilUsuarioViewAsync(int id);
        Task<bool> CrearUsuarioAsync(RegistroUsuarioDto usuario);
        Task<bool> ActualizarUsuarioAsync(EditarUsuarioDto usuario);
        Task<bool> ActualizarPerfilsync(PerfilUsuarioDto perfilUsuario);
        Task<bool> EliminarUsuarioAsync(int id);
        Task<bool> DesactivarUsuarioAsync(int id);
        Task<bool> ActivarUsuarioAsync(int id);
    }


    public class UsuarioService : IUsuarioService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UsuarioService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<UsuarioListaViewModel>> ObtenerUsuariosAsync()
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Usuario/Listar";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioListaViewModel>>();
                    return apiResponse?.Usuarios ?? new List<UsuarioListaViewModel>();
                }

                return new List<UsuarioListaViewModel>();
            }
            catch (Exception)
            {
                return new List<UsuarioListaViewModel>();
            }
        }


        /*
         * Metodo para mostrar la lista de usuario con paginacion y busqueda por nombre/cedula
         */

        public async Task<PaginacionResponse<UsuarioListaViewModel>> BuscarUsuariosRapidaAsync(
    string termino,
    int pagina = 1,
    int resultadosPorPagina = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    // Listar todos los usuarios con paginación
                    var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Usuario/ListarViewUsuario?pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                    var response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultadoPaginado = await response.Content.ReadFromJsonAsync<PaginacionResponse<UsuarioListaViewModel>>();

                        if (resultadoPaginado != null && resultadoPaginado.Success)
                        {
                            return resultadoPaginado;
                        }
                        else
                        {
                            return new PaginacionResponse<UsuarioListaViewModel>
                            {
                                Success = false,
                                Message = "No se pudieron obtener los usuarios"
                            };
                        }
                    }
                    else
                    {
                        return new PaginacionResponse<UsuarioListaViewModel>
                        {
                            Success = false,
                            Message = $"Error al obtener los usuarios: {response.StatusCode}"
                        };
                    }
                }

                // Buscar usuarios por término (nombre o cédula)
                var buscarUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Usuario/Busqueda-Usuario?termino={Uri.EscapeDataString(termino)}&pagina={pagina}&resultadoPorPagina={resultadosPorPagina}";
                var buscarResponse = await _httpClient.GetAsync(buscarUrl);

                if (buscarResponse.IsSuccessStatusCode)
                {
                    var result = await buscarResponse.Content.ReadFromJsonAsync<PaginacionResponse<UsuarioListaViewModel>>();
                    return result ?? new PaginacionResponse<UsuarioListaViewModel>
                    {
                        Success = false,
                        Message = "No se pudieron procesar los resultados"
                    };
                }
                else
                {
                    return new PaginacionResponse<UsuarioListaViewModel>
                    {
                        Success = false,
                        Message = $"Error en la búsqueda: {buscarResponse.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaginacionResponse<UsuarioListaViewModel>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }

        public async Task<List<string>> ObtenerRolesDeUsuarioAsync(int idUsuario)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Rol/ObtenerRolesDeUsuario/{idUsuario}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var roles = await response.Content.ReadFromJsonAsync<List<string>>();
                    return roles ?? new List<string>();
                }
                return new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }


        //public async Task<UsuarioListaViewModel> ObtenerUsuarioPorIdAsync(int id)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Usuario/Obtener/{id}";
        //        var response = await _httpClient.GetAsync(apiUrl);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var result = await response.Content.ReadFromJsonAsync<dynamic>();
        //            return result?.usuario?.ToObject<UsuarioListaViewModel>();
        //        }

        //        return null;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        public async Task<UsuarioListaViewModel?> ObtenerUsuarioPorIdAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Usuario/Obtener/{id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<UsuarioWrapper>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (wrapper?.Usuario != null)
                        return wrapper.Usuario;
                }

                // Fallback: si /Obtener no trae el esquema esperado, uso /Listar y filtro
                var lista = await ObtenerUsuariosAsync();
                return lista?.FirstOrDefault(u => u.IdUsuario == id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CrearUsuarioAsync(RegistroUsuarioDto usuario)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Usuario/Registro";

                var json = JsonSerializer.Serialize(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ActualizarUsuarioAsync(EditarUsuarioDto usuario)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Usuario/Editar";

                var json = JsonSerializer.Serialize(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

      
        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Usuario/Eliminar/{id}";

                var response = await _httpClient.DeleteAsync(apiUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DesactivarUsuarioAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Usuario/Desactivar?id={id}";

                var response = await _httpClient.DeleteAsync(apiUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ActivarUsuarioAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Usuario/Activar?id={id}";

                var response = await _httpClient.PatchAsync(apiUrl, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PerfilUsuarioDto> PerfilUsuarioViewAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Usuario/PerfilUsuario?idUsuario={id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var perfil = await response.Content.ReadFromJsonAsync<PerfilUsuarioDto>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return perfil;
                }
                else
                {
                    // Si la API retorna error, lanzar excepción con detalles
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error al obtener perfil: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Relanzar la excepción HTTP específica
                throw;
            }
            catch (Exception ex)
            {
                // Lanzar cualquier otra excepción
                throw new Exception($"Error inesperado al obtener perfil del usuario {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> ActualizarPerfilsync(PerfilUsuarioDto perfilUsuario)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Usuario/EditarPerfil";

                var json = JsonSerializer.Serialize(perfilUsuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class UsuarioWrapper
    {
        public UsuarioListaViewModel? Usuario { get; set; }
        public string? Msj { get; set; }
        public bool Resultado { get; set; }
    }

}
