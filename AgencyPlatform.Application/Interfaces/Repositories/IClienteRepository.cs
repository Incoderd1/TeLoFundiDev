using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IClienteRepository
    {
        Task<cliente> GetByIdAsync(int id);
        Task<cliente> GetByUsuarioIdAsync(int usuarioId);
        Task<bool> ExisteNicknameAsync(string nickname);
        Task AddAsync(cliente entity);
        Task UpdateAsync(cliente entity);
        Task DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
