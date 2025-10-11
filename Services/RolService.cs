using AppWebBiblioteca.Models;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class RolService : IRolService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RolService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<RolDto>> ObtenerRolesAsync()
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Roles/ObtenerRoles";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var roles = await response.Content.ReadFromJsonAsync<List<RolDto>>();
                    return roles ?? new List<RolDto>();
                }
                return new List<RolDto>();
            }
            catch
            {
                return new List<RolDto>();
            }
        }

        public async Task<(bool ok, string? mensaje)> AsignarRolAUsuarioAsync(AsignacionRolDto dto)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"]
                           + $"/Roles/AsignarRolAUsuario?idUsuario={dto.IdUsuario}&idRol={dto.IdRol}";

                using var resp = await _httpClient.PostAsync(apiUrl, content: null);

                string? mensaje = null;
                try
                {
                    var parsed = await resp.Content.ReadFromJsonAsync<ApiMensajeResponse>();
                    mensaje = parsed?.Mensaje;
                }
                catch { }

                return (resp.IsSuccessStatusCode, mensaje);
            }
            catch
            {
                return (false, "Error de conexión al asignar rol.");
            }
        }

        public async Task<(bool ok, string? mensaje)> QuitarRolAUsuarioAsync(AsignacionRolDto dto)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"]
                           + $"/Roles/QuitarRolAUsuario?idUsuario={dto.IdUsuario}&idRol={dto.IdRol}";

                using var resp = await _httpClient.PostAsync(apiUrl, content: null);

                string? mensaje = null;
                try
                {
                    var parsed = await resp.Content.ReadFromJsonAsync<ApiMensajeResponse>();
                    mensaje = parsed?.Mensaje;
                }
                catch { }

                return (resp.IsSuccessStatusCode, mensaje);
            }
            catch
            {
                return (false, "Error de conexión al quitar rol.");
            }
        }
    }

    // Para leer { mensaje: "..." } de tu API
    public class ApiMensajeResponse
    {
        public string? Mensaje { get; set; }
    }
}
