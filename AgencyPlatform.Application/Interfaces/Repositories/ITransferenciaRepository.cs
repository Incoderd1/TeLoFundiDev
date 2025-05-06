using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ITransferenciaRepository
    {
        Task<transferencia> GetByIdAsync(int id);
        Task<List<transferencia>> GetAllAsync();
        Task<transferencia> AddAsync(transferencia transferencia);
        Task UpdateAsync(transferencia transferencia);
        Task DeleteAsync(transferencia transferencia);
        Task SaveChangesAsync();

        Task<List<transferencia>> GetByAcompananteIdAsync(int acompananteId);
        Task<List<transferencia>> GetByAgenciaIdAsync(int agenciaId);
        Task<List<transferencia>> GetByClienteIdAsync(int clienteId);
        Task<List<transferencia>> GetByTransaccionIdAsync(int transaccionId);
        Task<decimal> GetSaldoByUsuarioAsync(int usuarioId, string tipoUsuario);
    }
}
