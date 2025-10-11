using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public interface IRolService
    {
        Task<List<RolDto>> ObtenerRolesAsync();
        Task<(bool ok, string? mensaje)> AsignarRolAUsuarioAsync(AsignacionRolDto dto);
        Task<(bool ok, string? mensaje)> QuitarRolAUsuarioAsync(AsignacionRolDto dto);
    }
}
