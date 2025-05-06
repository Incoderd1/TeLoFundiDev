using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IAnuncioDestacadoRepository :  IGenericRepository<anuncios_destacado>
    {
        Task<List<anuncios_destacado>> GetAllAsync();
        Task<anuncios_destacado?> GetByIdAsync(int id);
        Task<List<anuncios_destacado>> GetByAgenciaIdAsync(int agenciaId);
        Task<List<anuncios_destacado>> GetByAcompananteIdAsync(int acompananteId);
        Task<List<anuncios_destacado>> GetActivosAsync();
        Task AddAsync(anuncios_destacado entity);
        Task UpdateAsync(anuncios_destacado entity);
        Task DeleteAsync(anuncios_destacado entity);
        Task SaveChangesAsync();

        Task<int> CountActivosByAgenciaIdAsync(int agenciaId);


        //nuevas
        Task<int> DesactivarAnunciosVencidosAsync(DateTime fecha);
        Task<int> ActivarAnunciosProgramadosAsync(DateTime fecha);
        Task<bool> TieneAnuncioActivoAsync(int acompananteId);

        
        Task<List<anuncios_destacado>> GetActivosPorTipoYFechasAsync(string tipo, DateTime fechaInicio, DateTime fechaFin);
        Task<List<anuncios_destacado>> GetAnunciosDestacadosByAgenciaIdAsync(int agenciaId);

        Task<anuncios_destacado> GetByReferenceIdAsync(string referenceId); // Nuevo método




      
    }
}
