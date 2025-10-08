using AppWebBiblioteca.Models;
using System.Text;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
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

        public async Task<UsuarioListaViewModel> ObtenerUsuarioPorIdAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Usuario/Obtener/{id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return result?.usuario?.ToObject<UsuarioListaViewModel>();
                }

                return null;
            }
            catch (Exception)
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
    }
}
