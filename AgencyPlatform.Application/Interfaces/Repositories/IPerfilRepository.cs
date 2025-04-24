using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IPerfilRepository
    {
        // Métodos de IAcompananteRepository
        Task<acompanante> GetByIdAsync(int id);
        Task<List<acompanante>> GetByIdsAsync(List<int> ids);
        Task<List<int>> GetIdsPopularesAsync(int cantidad, List<int> excludeIds = null);
        Task<Dictionary<int, PerfilEstadisticasDto>> GetEstadisticasMultiplesAsync(List<int> perfilesIds);
        Task<List<int>> GetIdsByCategoriasAsync(List<int> categoriaIds, int cantidad, List<int> excludeIds = null);
        Task<List<int>> GetIdsByCiudadesAsync(List<string> ciudades, int cantidad, List<int> excludeIds = null);
        Task<List<int>> GetCategoriasIdsDePerfilAsync(int perfilId);
        Task<List<int>> GetCategoriasDePerfilesAsync(List<int> perfilesIds);
        Task<List<string>> GetCiudadesDePerfilesAsync(List<int> perfilesIds);
        Task<List<int>> GetPerfilesVisitadosIdsByClienteAsync(int clienteId);

        // Métodos de IVisitaRepository
        Task<List<visitas_perfil>> GetByClienteIdAsync(int clienteId, int cantidad);
        Task<List<int>> GetPerfilesVisitadosRecientementeIdsByClienteAsync(int clienteId, int cantidad);
        Task<int> CountByAcompananteIdAsync(int acompananteId);
        Task AddVisitaAsync(visitas_perfil visita);
        Task SaveVisitaChangesAsync();

        // Métodos de IContactoRepository
        Task AddContactoAsync(contacto contacto);
        Task SaveContactoChangesAsync();
    }
}
