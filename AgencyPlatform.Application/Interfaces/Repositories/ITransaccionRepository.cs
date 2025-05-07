using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ITransaccionRepository
    {
        Task<transaccion> GetByIdAsync(int id);
        Task<List<transaccion>> GetAllAsync();
        Task<transaccion> AddAsync(transaccion transaccion);
        Task UpdateAsync(transaccion transaccion);
        Task DeleteAsync(transaccion transaccion);
        Task SaveChangesAsync();

        Task<List<transaccion>> GetByClienteIdAsync(int clienteId);
        Task<List<transaccion>> GetByAcompananteIdAsync(int acompananteId);
        Task<List<transaccion>> GetByAgenciaIdAsync(int agenciaId);
        Task<transaccion> GetByExternalIdAsync(string externalId);
        Task<decimal> GetTotalPagadoByClienteIdAsync(int clienteId);

        Task<List<transaccion>> GetByAcompananteIdAsync(
        int acompananteId,
        DateTime? desde = null,
        DateTime? hasta = null,
        int pagina = 1,
        int elementosPorPagina = 10);
    }
}
