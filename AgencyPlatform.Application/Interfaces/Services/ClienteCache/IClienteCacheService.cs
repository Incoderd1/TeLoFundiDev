using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Cliente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.ClienteCache
{
    public interface IClienteCacheService
    {
        // Métodos para obtener datos con caché
        Task<List<CuponClienteDto>> GetCuponesDisponiblesAsync(int clienteId, Func<Task<List<CuponClienteDto>>> dataFetcher);
        Task<List<AcompananteCardDto>> GetPerfilesPopularesAsync(int cantidad, Func<Task<List<AcompananteCardDto>>> dataFetcher);
        Task<List<int>> GetCategoriasInteresAsync(int clienteId, List<int> perfilesVisitadosIds, Func<List<int>, Task<List<int>>> dataFetcher);
        Task<List<string>> GetCiudadesInteresAsync(int clienteId, List<int> perfilesVisitadosIds, Func<List<int>, Task<List<string>>> dataFetcher);

        // Métodos para invalidar caché
        void InvalidarCacheCliente(int clienteId);
        void InvalidarCachePerfilesPopulares();
    }
}
