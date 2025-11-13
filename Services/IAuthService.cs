using AppWebBiblioteca.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AppWebBiblioteca.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(Login login);
        Task LogoutAsync();
        bool IsAuthenticated();
        string GetToken();
        string GetUserEmail();
        List<string> GetUserRoles();
        int? GetUserId();
    }

    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AuthResponse> LoginAsync(Login login)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Usuario/login";

                var json = JsonSerializer.Serialize(login);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (authResponse != null && authResponse.Resultado)
                {
                    // Guardar en sesión (opcional, si aún quieres usar sesión)
                    _httpContextAccessor.HttpContext.Session.SetString("JWTToken", authResponse.Token);
                    _httpContextAccessor.HttpContext.Session.SetString("UserEmail", authResponse.Email);
                    _httpContextAccessor.HttpContext.Session.SetString("UserName", authResponse.Nombre);

                    var rolesJson = JsonSerializer.Serialize(authResponse.Roles);
                    _httpContextAccessor.HttpContext.Session.SetString("UserRoles", rolesJson);

                    // CREAR LAS CLAIMS
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authResponse.idUsuario.ToString()),
                new Claim(ClaimTypes.Email, authResponse.Email),
                new Claim(ClaimTypes.Name, authResponse.Nombre),
                new Claim("JWTToken", authResponse.Token) // Claim personalizada para el token
            };

                    // Agregar cada rol como claim
                    if (authResponse.Roles != null)
                    {
                        foreach (var rol in authResponse.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, rol));
                        }
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Hacer login con las claims
                    await _httpContextAccessor.HttpContext.SignInAsync(
      CookieAuthenticationDefaults.AuthenticationScheme, // ← Usar este scheme
      claimsPrincipal,
      new AuthenticationProperties
      {
          IsPersistent = false,
          ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
      });
                }

                return authResponse ?? new AuthResponse { Resultado = false, Msj = "Error procesando la respuesta" };
            }
            catch (HttpRequestException ex)
            {
                return new AuthResponse { Resultado = false, Msj = "Error de conexión con el servidor" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Resultado = false, Msj = $"Error: {ex.Message}" };
            }
        }

        public int? GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        //public async Task<AuthResponse> LoginAsync(Login login)
        //{
        //    try
        //    {
        //        var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/Usuario/login";

        //        var json = JsonSerializer.Serialize(login);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = await _httpClient.PostAsync(apiUrl, content);

        //        // Leer la respuesta sin importar el status code
        //        var responseContent = await response.Content.ReadAsStringAsync();

        //        // Intentar deserializar la respuesta
        //        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent,
        //            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //        if (authResponse != null)
        //        {
        //            if (authResponse.Resultado)
        //            {
        //                // Guardar en sesión solo si el login fue exitoso
        //                _httpContextAccessor.HttpContext.Session.SetString("JWTToken", authResponse.Token);
        //                _httpContextAccessor.HttpContext.Session.SetString("UserEmail", authResponse.Email);
        //                _httpContextAccessor.HttpContext.Session.SetString("UserName", authResponse.Nombre);

        //                // Guardar roles como JSON
        //                var rolesJson = JsonSerializer.Serialize(authResponse.Roles);
        //                _httpContextAccessor.HttpContext.Session.SetString("UserRoles", rolesJson);
        //            }

        //            return authResponse; // ← Siempre devolver la respuesta de la API
        //        }
        //        else
        //        {
        //            // Si no se pudo deserializar, devolver error genérico
        //            return new AuthResponse
        //            {
        //                Resultado = false,
        //                Msj = response.IsSuccessStatusCode ?
        //                    "Error procesando la respuesta" :
        //                    "Credenciales inválidas"
        //            };
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // Error de conexión con la API
        //        return new AuthResponse { Resultado = false, Msj = "Error de conexión con el servidor" };
        //    }
        //    catch (Exception ex)
        //    {
        //        // Error inesperado
        //        return new AuthResponse { Resultado = false, Msj = $"Error: {ex.Message}" };
        //    }
        //}

        //public async Task LogoutAsync()
        //{
        //    _httpContextAccessor.HttpContext.Session.Remove("JWTToken");
        //    _httpContextAccessor.HttpContext.Session.Remove("UserEmail");
        //    _httpContextAccessor.HttpContext.Session.Remove("UserName");
        //    _httpContextAccessor.HttpContext.Session.Remove("UserRoles");
        //    await Task.CompletedTask;
        //}

        public async Task LogoutAsync()
        {
            // Limpiar sesión
            _httpContextAccessor.HttpContext.Session.Clear();

            // Hacer logout de la autenticación
            await _httpContextAccessor.HttpContext.SignOutAsync("Cookies");
        }

        //public bool IsAuthenticated()
        //{
        //    var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
        //    return !string.IsNullOrEmpty(token);
        //}
        public bool IsAuthenticated()
        {
            // Ahora verificamos tanto la sesión como la autenticación por cookies
            var hasSessionToken = !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("JWTToken"));
            var isCookieAuthenticated = _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;

            return hasSessionToken && isCookieAuthenticated;
        }

        public string GetToken()
        {
            return _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
        }

        public string GetUserEmail()
        {
            return _httpContextAccessor.HttpContext.Session.GetString("UserEmail");
        }

        public List<string> GetUserRoles()
        {
            var rolesJson = _httpContextAccessor.HttpContext.Session.GetString("UserRoles");
            if (!string.IsNullOrEmpty(rolesJson))
            {
                return JsonSerializer.Deserialize<List<string>>(rolesJson);
            }
            return new List<string>();
        }
    }
}