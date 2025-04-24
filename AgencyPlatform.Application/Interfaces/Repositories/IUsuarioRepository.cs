using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgencyPlatform.Core;


namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IUsuarioRepository
    {
        Task<usuario?> ObtenerPorEmailAsync(string email);
        Task<usuario?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<usuario>> ObtenerTodosAsync(int page, int pageSize);
        Task CrearAsync(usuario usuario);
        Task EliminarAsync(usuario usuario);
        Task GuardarCambiosAsync();
        Task<usuario> GetByIdAsync(int id);


        Task<string> GetRolNameByUserIdAsync(int usuarioId);

        Task<List<string>> GetRolesAsync(int usuarioId);


    }
}
