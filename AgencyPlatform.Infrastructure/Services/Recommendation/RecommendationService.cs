using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services.ClienteCache;
using AgencyPlatform.Application.Interfaces.Services.Recommendation;
using AgencyPlatform.Shared.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Recommendation
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IVisitaRepository _visitaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RecommendationService> _logger;
        private readonly IClienteCacheService _cacheService;

        public RecommendationService(
            IAcompananteRepository acompananteRepository,
            IVisitaRepository visitaRepository,
            IClienteRepository clienteRepository,
            IMapper mapper,
            ILogger<RecommendationService> logger,
            IClienteCacheService cacheService)
        {
            _acompananteRepository = acompananteRepository ?? throw new ArgumentNullException(nameof(acompananteRepository));
            _visitaRepository = visitaRepository ?? throw new ArgumentNullException(nameof(visitaRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<List<AcompananteCardDto>> GetPerfilesVisitadosRecientementeAsync(int clienteId, int cantidad = 5)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            // 1. Obtenemos los IDs de perfiles visitados con una sola consulta
            var perfilesIds = await _visitaRepository.GetPerfilesVisitadosRecientementeIdsByClienteAsync(clienteId, cantidad);
            if (!perfilesIds.Any())
                return new List<AcompananteCardDto>();

            // 2. Cargamos todos los perfiles en una sola consulta
            var acompanantes = await _acompananteRepository.GetByIdsAsync(perfilesIds);

            // 3. Cargamos estadísticas en operaciones por lotes
            var estadisticas = await _acompananteRepository.GetEstadisticasMultiplesAsync(perfilesIds);

            // 4. Mapeamos a DTOs usando datos precargados
            var perfiles = new List<AcompananteCardDto>();
            foreach (var acompanante in acompanantes.Where(a => a.esta_disponible == true))
            {
                var perfilDto = _mapper.Map<AcompananteCardDto>(acompanante);

                // Usar estadísticas precargadas
                if (estadisticas.TryGetValue(acompanante.id, out var stats))
                {
                    perfilDto.TotalVisitas = stats.TotalVisitas;
                    perfilDto.TotalContactos = stats.TotalContactos;
                    perfilDto.FotoPrincipalUrl = stats.FotoPrincipalUrl;
                }

                perfiles.Add(perfilDto);
                if (perfiles.Count >= cantidad)
                    break;
            }

            return perfiles;
        }

        public async Task<List<AcompananteCardDto>> GetPerfilesRecomendadosAsync(int clienteId, int cantidad = 5)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            // 1. Obtener perfiles visitados de forma optimizada
            var perfilesVisitadosIds = await _acompananteRepository.GetPerfilesVisitadosIdsByClienteAsync(clienteId);
            if (!perfilesVisitadosIds.Any())
            {
                // Si no hay historial, devolver populares directamente
                return await GetPerfilesPopularesAsync(cantidad);
            }

            // 2. Obtener intereses (categorías y ciudades) usando métodos con caché
            var categoriasInteres = await GetCategoriasInteresAsync(clienteId, perfilesVisitadosIds);
            var ciudadesInteres = await GetCiudadesInteresAsync(clienteId, perfilesVisitadosIds);

            // 3. Obtener candidatos a recomendar (sin consultas individuales)
            var candidatosIds = new List<int>();

            // Perfiles por categorías
            if (categoriasInteres.Any())
            {
                var perfilesPorCategoriaIds = await _acompananteRepository.GetIdsByCategoriasAsync(
                    categoriasInteres, 20, perfilesVisitadosIds);
                candidatosIds.AddRange(perfilesPorCategoriaIds);
            }

            // Perfiles por ciudades
            if (ciudadesInteres.Any() && candidatosIds.Count < cantidad * 2)
            {
                var perfilesPorCiudadIds = await _acompananteRepository.GetIdsByCiudadesAsync(
                    ciudadesInteres, 20, perfilesVisitadosIds);
                candidatosIds.AddRange(perfilesPorCiudadIds.Where(id => !candidatosIds.Contains(id)));
            }

            // Si no hay suficientes, agregar populares
            if (candidatosIds.Count < cantidad)
            {
                var perfilesPopularesIds = await _acompananteRepository.GetIdsPopularesAsync(
                    20, perfilesVisitadosIds);
                candidatosIds.AddRange(perfilesPopularesIds.Where(id => !candidatosIds.Contains(id)));
            }

            // 4. Limitar candidatos y obtener datos completos en pocas consultas
            candidatosIds = candidatosIds.Take(cantidad).ToList();
            if (!candidatosIds.Any())
                return new List<AcompananteCardDto>();

            var acompanantes = await _acompananteRepository.GetByIdsAsync(candidatosIds);
            var estadisticas = await _acompananteRepository.GetEstadisticasMultiplesAsync(candidatosIds);

            // Precargamos todas las categorías de los perfiles candidatos en una sola consulta
            var todasLasCategorias = new Dictionary<int, List<int>>();
            foreach (var id in candidatosIds)
            {
                todasLasCategorias[id] = await _acompananteRepository.GetCategoriasIdsDePerfilAsync(id);
            }

            // 5. Mapear resultados con información precargada
            var resultado = new List<AcompananteCardDto>();
            foreach (var acompanante in acompanantes.Where(a => a.esta_disponible == true))
            {
                var perfilDto = _mapper.Map<AcompananteCardDto>(acompanante);

                // Usar estadísticas precargadas
                if (estadisticas.TryGetValue(acompanante.id, out var stats))
                {
                    perfilDto.TotalVisitas = stats.TotalVisitas;
                    perfilDto.TotalContactos = stats.TotalContactos;
                    perfilDto.FotoPrincipalUrl = stats.FotoPrincipalUrl;
                }

                // Determinar razón de recomendación (sin consultas adicionales)
                var categoriasPerfil = todasLasCategorias[acompanante.id];
                if (categoriasInteres.Any() && categoriasPerfil.Any(c => categoriasInteres.Contains(c)))
                {
                    perfilDto.RazonRecomendacion = "Basado en tus intereses";
                }
                else if (ciudadesInteres.Contains(acompanante.ciudad))
                {
                    perfilDto.RazonRecomendacion = $"En {acompanante.ciudad}";
                }
                else
                {
                    perfilDto.RazonRecomendacion = "Popular en la plataforma";
                }

                resultado.Add(perfilDto);
            }

            _logger.LogDebug("Recomendaciones generadas para cliente {ClienteId}. Cantidad: {Cantidad}", clienteId, resultado.Count);
            return resultado;
        }

        public async Task<List<AcompananteCardDto>> GetPerfilesPopularesAsync(int cantidad = 5)
        {
            // Usar el servicio de caché para obtener perfiles populares
            return await _cacheService.GetPerfilesPopularesAsync(cantidad, async () =>
            {
                // Esta función lambda se ejecutará solo si los datos no están en caché
                _logger.LogDebug("Consultando perfiles populares en base de datos. Cantidad: {Cantidad}", cantidad);

                var perfilesIds = await _acompananteRepository.GetIdsPopularesAsync(cantidad);
                var acompanantes = await _acompananteRepository.GetByIdsAsync(perfilesIds);
                var estadisticas = await _acompananteRepository.GetEstadisticasMultiplesAsync(perfilesIds);

                var resultado = new List<AcompananteCardDto>();
                foreach (var acompanante in acompanantes.Where(a => a.esta_disponible == true))
                {
                    var perfilDto = _mapper.Map<AcompananteCardDto>(acompanante);

                    if (estadisticas.TryGetValue(acompanante.id, out var stats))
                    {
                        perfilDto.TotalVisitas = stats.TotalVisitas;
                        perfilDto.TotalContactos = stats.TotalContactos;
                        perfilDto.FotoPrincipalUrl = stats.FotoPrincipalUrl;
                    }

                    perfilDto.RazonRecomendacion = "Popular en la plataforma";
                    resultado.Add(perfilDto);
                }

                return resultado;
            });
        }

        private async Task<List<int>> GetCategoriasInteresAsync(int clienteId, List<int> perfilesVisitadosIds)
        {
            // Usar el servicio de caché para obtener categorías de interés
            return await _cacheService.GetCategoriasInteresAsync(
                clienteId,
                perfilesVisitadosIds,
                async (ids) =>
                {
                    // Esta función lambda se ejecutará solo si los datos no están en caché
                    return await _acompananteRepository.GetCategoriasDePerfilesAsync(ids);
                });
        }

        private async Task<List<string>> GetCiudadesInteresAsync(int clienteId, List<int> perfilesVisitadosIds)
        {
            // Usar el servicio de caché para obtener ciudades de interés
            return await _cacheService.GetCiudadesInteresAsync(
                clienteId,
                perfilesVisitadosIds,
                async (ids) =>
                {
                    // Esta función lambda se ejecutará solo si los datos no están en caché
                    return await _acompananteRepository.GetCiudadesDePerfilesAsync(ids);
                });
        }
    }
}


