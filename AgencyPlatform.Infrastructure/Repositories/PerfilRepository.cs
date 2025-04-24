using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class PerfilRepository : IPerfilRepository
    {
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IVisitaRepository _visitaRepository;
        private readonly IContactoRepository _contactoRepository;

        public PerfilRepository(
            IAcompananteRepository acompananteRepository,
            IVisitaRepository visitaRepository,
            IContactoRepository contactoRepository)
        {
            _acompananteRepository = acompananteRepository ?? throw new ArgumentNullException(nameof(acompananteRepository));
            _visitaRepository = visitaRepository ?? throw new ArgumentNullException(nameof(visitaRepository));
            _contactoRepository = contactoRepository ?? throw new ArgumentNullException(nameof(contactoRepository));
        }

        // Implementación de IAcompananteRepository
        public Task<acompanante> GetByIdAsync(int id) => _acompananteRepository.GetByIdAsync(id);
        public Task<List<acompanante>> GetByIdsAsync(List<int> ids) => _acompananteRepository.GetByIdsAsync(ids);
        public Task<List<int>> GetIdsPopularesAsync(int cantidad, List<int> excludeIds = null) =>
            _acompananteRepository.GetIdsPopularesAsync(cantidad, excludeIds);
        public async Task<Dictionary<int, PerfilEstadisticasDto>> GetEstadisticasMultiplesAsync(List<int> perfilesIds)
        {
            // Obtenemos el diccionario original con AcompananteEstadisticas
            var originalEstadisticas = await _acompananteRepository.GetEstadisticasMultiplesAsync(perfilesIds);

            // Convertimos a un nuevo diccionario con PerfilEstadisticasDto
            var resultado = new Dictionary<int, PerfilEstadisticasDto>();

            foreach (var kvp in originalEstadisticas)
            {
                resultado[kvp.Key] = new PerfilEstadisticasDto
                {
                    TotalVisitas = kvp.Value.TotalVisitas,
                    TotalContactos = kvp.Value.TotalContactos,
                    FotoUrl = kvp.Value.FotoPrincipalUrl
                    // Añade aquí otras propiedades si PerfilEstadisticasDto tiene más campos
                };
            }

            return resultado;
        }
        public Task<List<int>> GetIdsByCategoriasAsync(List<int> categoriaIds, int cantidad, List<int> excludeIds = null) =>
            _acompananteRepository.GetIdsByCategoriasAsync(categoriaIds, cantidad, excludeIds);
        public Task<List<int>> GetIdsByCiudadesAsync(List<string> ciudades, int cantidad, List<int> excludeIds = null) =>
            _acompananteRepository.GetIdsByCiudadesAsync(ciudades, cantidad, excludeIds);
        public Task<List<int>> GetCategoriasIdsDePerfilAsync(int perfilId) =>
            _acompananteRepository.GetCategoriasIdsDePerfilAsync(perfilId);
        public Task<List<int>> GetCategoriasDePerfilesAsync(List<int> perfilesIds) =>
            _acompananteRepository.GetCategoriasDePerfilesAsync(perfilesIds);
        public Task<List<string>> GetCiudadesDePerfilesAsync(List<int> perfilesIds) =>
            _acompananteRepository.GetCiudadesDePerfilesAsync(perfilesIds);
        public Task<List<int>> GetPerfilesVisitadosIdsByClienteAsync(int clienteId) =>
            _acompananteRepository.GetPerfilesVisitadosIdsByClienteAsync(clienteId);

        // Implementación de IVisitaRepository
        public Task<List<visitas_perfil>> GetByClienteIdAsync(int clienteId, int cantidad) =>
            _visitaRepository.GetByClienteIdAsync(clienteId, cantidad);
        public Task<List<int>> GetPerfilesVisitadosRecientementeIdsByClienteAsync(int clienteId, int cantidad) =>
            _visitaRepository.GetPerfilesVisitadosRecientementeIdsByClienteAsync(clienteId, cantidad);
        public Task<int> CountByAcompananteIdAsync(int acompananteId) =>
            _contactoRepository.CountByAcompananteIdAsync(acompananteId);
        public Task AddVisitaAsync(visitas_perfil visita) =>
            _visitaRepository.AddAsync(visita);
        public Task SaveVisitaChangesAsync() =>
            _visitaRepository.SaveChangesAsync();

        // Implementación de IContactoRepository
       
        public Task AddContactoAsync(contacto contacto) =>
            _contactoRepository.AddAsync(contacto);
        public Task SaveContactoChangesAsync() =>
            _contactoRepository.SaveChangesAsync();
    }
}
