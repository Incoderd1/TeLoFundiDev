using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ISolicitudRepository
    {
        Task<solicitud_agencia> GetByIdAsync(int solicitudId);
        Task AddAsync(solicitud_agencia solicitud);
        Task UpdateAsync(solicitud_agencia solicitud);
        Task DeleteAsync(solicitud_agencia solicitud);
        Task SaveChangesAsync();
    }
}
