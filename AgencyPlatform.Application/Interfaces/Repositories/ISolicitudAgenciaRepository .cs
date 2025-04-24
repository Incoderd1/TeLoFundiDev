using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ISolicitudAgenciaRepository
    {
        Task<solicitud_agencia> GetByIdAsync(int id);
        Task<List<solicitud_agencia>> GetAllAsync();
        Task<List<solicitud_agencia>> GetPendientesByAgenciaIdAsync(int agenciaId);
        Task<List<solicitud_agencia>> GetPendientesByAcompananteIdAsync(int acompananteId);
        Task<int> CountPendientesByAgenciaIdAsync(int agenciaId);
        Task<PaginatedResult<solicitud_agencia>> GetHistorialAsync(
            int? agenciaId = null,
            int? acompananteId = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string estado = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<bool> ExistePendienteAsync(int agenciaId, int acompananteId);
        Task AddAsync(solicitud_agencia solicitud);
        Task UpdateAsync(solicitud_agencia solicitud);
        Task<bool> SaveChangesAsync();
    }
}
