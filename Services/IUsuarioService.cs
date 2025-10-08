using AppWebBiblioteca.Models;

namespace AppWebBiblioteca.Services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioListaViewModel>> ObtenerUsuariosAsync();
        Task<UsuarioListaViewModel> ObtenerUsuarioPorIdAsync(int id);

        Task<bool> CrearUsuarioAsync(RegistroUsuarioDto usuario);
        Task<bool> ActualizarUsuarioAsync(EditarUsuarioDto usuario);
        Task<bool> EliminarUsuarioAsync(int id);
        Task<bool> DesactivarUsuarioAsync(int id);
        Task<bool> ActivarUsuarioAsync(int id);
    }
}
