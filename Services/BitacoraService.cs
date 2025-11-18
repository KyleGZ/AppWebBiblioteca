namespace AppWebBiblioteca.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BitacoraService(HttpClient httpClient, IConfiguration configuration)
        {
         _httpClient = httpClient;
        _configuration = configuration;
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

                    var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Bitacora/RegistrarAccion";

                    _ = _httpClient.PostAsJsonAsync(apiUrl, bitacoraData);

                    // Opcional: Podrías loggear que se envió la solicitud
                    Console.WriteLine($"Solicitud de bitácora enviada: {accion} para {tablaAfectada}:{idRegistro}");
                }
            }
            catch (Exception ex)
            {
                // Log silencioso - no debe afectar el flujo principal
                Console.WriteLine($"Error no crítico al enviar bitácora: {ex.Message}");
            }
        }



    }

    public interface IBitacoraService
    {
        Task RegistrarAccionAsync(int idUsuario, string accion, string tablaAfectada, int idRegistro);

    }

}
