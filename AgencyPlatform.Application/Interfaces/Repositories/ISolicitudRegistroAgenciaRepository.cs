using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ISolicitudRegistroAgenciaRepository
    {
        Task<List<solicitud_registro_agencia>> GetAllAsync();
        Task<solicitud_registro_agencia> GetByIdAsync(int id);
        Task<List<solicitud_registro_agencia>> GetSolicitudesPendientesAsync();
        Task<List<solicitud_registro_agencia>> GetSolicitudesByEstadoAsync(string estado);
        Task AddAsync(solicitud_registro_agencia solicitud);
        Task UpdateAsync(solicitud_registro_agencia solicitud);
        Task DeleteAsync(solicitud_registro_agencia solicitud);
        Task SaveChangesAsync();
    }
}
