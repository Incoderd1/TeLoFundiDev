using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IPagoVerificacionRepository
    {
        Task<pago_verificacion> GetByIdAsync(int id);
        Task<pago_verificacion> GetByVerificacionIdAsync(int verificacionId);
        Task<List<pago_verificacion>> GetByAcompananteIdAsync(int acompananteId);
        Task<List<pago_verificacion>> GetByAgenciaIdAsync(int agenciaId);
        Task<List<pago_verificacion>> GetPendientesByAgenciaIdAsync(int agenciaId);
        Task<bool> ExistenPagosCompletadosAsync(int acompananteId);
        Task AddAsync(pago_verificacion pago);
        Task UpdateAsync(pago_verificacion pago);
        Task DeleteAsync(pago_verificacion pago);
        Task SaveChangesAsync();
    }
}
