using AgencyPlatform.Application.DTOs.Usuarios;

namespace AgencyPlatform.Application.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioDto> RegistrarAsync(RegistroUsuarioDto dto);
        Task<AuthResponseDto> LoginAsync(LoginUsuarioDto dto);
        Task<UsuarioDto> ObtenerPorIdAsync(int id);
        Task<IEnumerable<UsuarioDto>> ObtenerTodosPaginadoAsync(int page, int pageSize);
        Task<UsuarioDto> ActualizarAsync(int id, UpdateUsuarioDto dto);
        Task<bool> EliminarAsync(int id);
        Task<AuthResponseDto> RenovarTokenAsync(RefreshTokenResponseDto dto, string ip, string userAgent);
        


    }
}
