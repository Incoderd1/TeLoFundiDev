using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IFotoRepository
    {
        Task<List<foto>> GetByAcompananteIdAsync(int acompananteId);
        Task<foto> GetByIdAsync(int id);
        Task AddAsync(foto entity);
        Task UpdateAsync(foto entity);
        Task DeleteAsync(foto entity);
        Task SaveChangesAsync();
        Task QuitarFotosPrincipalesAsync(int acompananteId);
        Task<int> ContarFotosPorAcompananteAsync(int acompananteId); 
    }
}
