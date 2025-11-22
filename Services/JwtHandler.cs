using System.Net.Http.Headers;

namespace AppWebBiblioteca.Services
{
    public class JwtHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public JwtHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var token = httpContext.Session.GetString("JWTToken");
                if (!string.IsNullOrEmpty(token))
                {
                    // Evitar comillas u otros caracteres
                    token = token.Trim().Trim('"');
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
