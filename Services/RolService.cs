using AppWebBiblioteca.Models;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public class RolService : IRolService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RolService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<RolDto>> ObtenerRolesAsync()
        {
            try
            {
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Rol/ObtenerRoles";
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
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"]
                           + $"/Rol/AsignarRolAUsuario?idUsuario={dto.IdUsuario}&idRol={dto.IdRol}";

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
                AgregarTokenAutenticacion();
                var apiUrl = _configuration["ApiSettings:BaseUrl"]
                           + $"/Rol/QuitarRolAUsuario?idUsuario={dto.IdUsuario}&idRol={dto.IdRol}";

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
