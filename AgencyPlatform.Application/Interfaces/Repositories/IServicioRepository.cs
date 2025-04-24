using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IServicioRepository
    {
        Task<List<servicio>> GetAllAsync();
        Task<servicio?> GetByIdAsync(int id);
        Task<List<servicio>> GetByAcompananteIdAsync(int acompananteId);
        Task AddAsync(servicio entity);
        Task UpdateAsync(servicio entity);
        Task DeleteAsync(servicio entity);
        Task SaveChangesAsync();
    }
}
