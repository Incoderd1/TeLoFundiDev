using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IVisitaPerfilRepository
    {
        Task<List<visitas_perfil>> GetByAcompananteIdAsync(int acompananteId);
        Task<visitas_perfil> GetByIdAsync(int id);
        Task AddAsync(visitas_perfil entity);
        Task UpdateAsync(visitas_perfil entity);
        Task DeleteAsync(visitas_perfil entity);
        Task SaveChangesAsync();
        Task<int> GetTotalByAcompananteIdAsync(int acompananteId);
        Task<int> GetTotalDesdeAsync(int acompananteId, DateTime fechaInicio);
        Task<Dictionary<DateTime, int>> GetVisitasPorDiaAsync(int acompananteId, int dias);
    }
}
