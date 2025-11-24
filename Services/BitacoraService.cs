namespace AppWebBiblioteca.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BitacoraService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task RegistrarAccionAsync(int userId, string accion, string tablaAfectada, int idRegistro)
        {
            try
            {

                if (userId > 0)
                {
                    var bitacoraData = new
                    {
                        IdUsuario = userId,
                        Accion = accion,
                        TablaAfectada = tablaAfectada,
                        IdRegistro = idRegistro
                    };

                    // No esperamos la respuesta, solo enviamos
                    AgregarTokenAutenticacion();
                    var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Bitacora/RegistrarAccion";

                    _ = _httpClient.PostAsJsonAsync(apiUrl, bitacoraData);

                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public interface IBitacoraService
    {
        Task RegistrarAccionAsync(int idUsuario, string accion, string tablaAfectada, int idRegistro);

    }

}
