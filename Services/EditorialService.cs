using AppWebBiblioteca.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace AppWebBiblioteca.Services
{
    public class EditorialService : IEditorialService
    {
        private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EditorialService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<EditorialDto>> ObtenerEditorialesAsync(string? nombre)
    {
        try
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/Editorial/Lista-Editoriales?nombre={nombre}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var editoriales = await response.Content.ReadFromJsonAsync<List<EditorialDto>>();
                return editoriales ?? new List<EditorialDto>();
            }
            return new List<EditorialDto>();
        }
        catch
        {
            return new List<EditorialDto>();
        }
    }
}

public interface IEditorialService
{
    Task<List<EditorialDto>> ObtenerEditorialesAsync(string? nombre);
}
}
