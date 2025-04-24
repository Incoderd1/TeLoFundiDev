using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.Interfaces.Services.ClienteCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.ClienteCache
{
    public class ClienteCacheService : IClienteCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ClienteCacheService> _logger;
        private static readonly SemaphoreSlim _cacheSemaphore = new SemaphoreSlim(1, 1);

        // Cache keys
        private static class CacheKeys
        {
            public static string PerfilesPopulares(int cantidad) => $"perfiles_populares_{cantidad}";
            public static string CategoriasInteres(int clienteId) => $"categorias_interes_{clienteId}";
            public static string CiudadesInteres(int clienteId) => $"ciudades_interes_{clienteId}";
            public static string CuponesDisponibles(int clienteId) => $"cupones_disponibles_{clienteId}";
        }

        public ClienteCacheService(
            IMemoryCache cache,
            ILogger<ClienteCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<CuponClienteDto>> GetCuponesDisponiblesAsync(int clienteId, Func<Task<List<CuponClienteDto>>> dataFetcher)
        {
            var cacheKey = CacheKeys.CuponesDisponibles(clienteId);

            // Intentar obtener del caché
            if (_cache.TryGetValue(cacheKey, out List<CuponClienteDto> cachedResult))
            {
                _logger.LogDebug("Cupones disponibles obtenidos desde caché. Cliente: {ClienteId}", clienteId);
                return cachedResult;
            }

            // Prevenir cache stampede con un semáforo
            try
            {
                await _cacheSemaphore.WaitAsync();

                // Revisar de nuevo en caso de que otro thread haya cargado los datos mientras esperábamos
                if (_cache.TryGetValue(cacheKey, out cachedResult))
                {
                    return cachedResult;
                }

                // Si no está en caché, obtener de la base de datos
                _logger.LogDebug("Consultando cupones disponibles en base de datos. Cliente: {ClienteId}", clienteId);
                var result = await dataFetcher();

                // Guardar en caché por 5 minutos (tiempo corto ya que pueden cambiar frecuentemente)
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, result, cacheOptions);
                _logger.LogDebug("Cupones disponibles guardados en caché. Cliente: {ClienteId}, Cantidad: {Cantidad}",
                    clienteId, result.Count);

                return result;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        public async Task<List<AcompananteCardDto>> GetPerfilesPopularesAsync(int cantidad, Func<Task<List<AcompananteCardDto>>> dataFetcher)
        {
            var cacheKey = CacheKeys.PerfilesPopulares(cantidad);

            // Intentar obtener del caché
            if (_cache.TryGetValue(cacheKey, out List<AcompananteCardDto> cachedResult))
            {
                _logger.LogDebug("Perfiles populares obtenidos desde caché. Cantidad: {Cantidad}", cantidad);
                return cachedResult;
            }

            try
            {
                await _cacheSemaphore.WaitAsync();

                // Revisar de nuevo en caso de que otro thread haya cargado los datos mientras esperábamos
                if (_cache.TryGetValue(cacheKey, out cachedResult))
                {
                    return cachedResult;
                }

                // Si no están en caché, realizar la consulta a través del dataFetcher
                _logger.LogDebug("Consultando perfiles populares en base de datos. Cantidad: {Cantidad}", cantidad);
                var resultado = await dataFetcher();

                // Guardar el resultado en caché para futuras solicitudes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15)) // Se expira después de 15 minutos
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, resultado, cacheOptions);
                _logger.LogDebug("Perfiles populares guardados en caché. Cantidad: {Cantidad}", resultado.Count);

                return resultado;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        public async Task<List<int>> GetCategoriasInteresAsync(int clienteId, List<int> perfilesVisitadosIds, Func<List<int>, Task<List<int>>> dataFetcher)
        {
            var cacheKey = CacheKeys.CategoriasInteres(clienteId);

            if (_cache.TryGetValue(cacheKey, out List<int> cachedResult))
            {
                _logger.LogDebug("Categorías de interés obtenidas desde caché. Cliente: {ClienteId}", clienteId);
                return cachedResult;
            }

            try
            {
                await _cacheSemaphore.WaitAsync();

                if (_cache.TryGetValue(cacheKey, out cachedResult))
                {
                    return cachedResult;
                }

                var categorias = await dataFetcher(perfilesVisitadosIds);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, categorias, cacheOptions);
                _logger.LogDebug("Categorías de interés guardadas en caché. Cliente: {ClienteId}, Cantidad: {Cantidad}",
                    clienteId, categorias.Count);

                return categorias;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        public async Task<List<string>> GetCiudadesInteresAsync(int clienteId, List<int> perfilesVisitadosIds, Func<List<int>, Task<List<string>>> dataFetcher)
        {
            var cacheKey = CacheKeys.CiudadesInteres(clienteId);

            if (_cache.TryGetValue(cacheKey, out List<string> cachedResult))
            {
                _logger.LogDebug("Ciudades de interés obtenidas desde caché. Cliente: {ClienteId}", clienteId);
                return cachedResult;
            }

            try
            {
                await _cacheSemaphore.WaitAsync();

                if (_cache.TryGetValue(cacheKey, out cachedResult))
                {
                    return cachedResult;
                }

                var ciudades = await dataFetcher(perfilesVisitadosIds);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, ciudades, cacheOptions);
                _logger.LogDebug("Ciudades de interés guardadas en caché. Cliente: {ClienteId}, Cantidad: {Cantidad}",
                    clienteId, ciudades.Count);

                return ciudades;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        public void InvalidarCacheCliente(int clienteId)
        {
            _cache.Remove(CacheKeys.CategoriasInteres(clienteId));
            _cache.Remove(CacheKeys.CiudadesInteres(clienteId));
            _cache.Remove(CacheKeys.CuponesDisponibles(clienteId));
            _logger.LogDebug("Caché invalidada para el cliente {ClienteId}", clienteId);
        }

        public void InvalidarCachePerfilesPopulares()
        {
            // Invalidar las cachés más comunes
            for (int i = 5; i <= 20; i += 5)
            {
                _cache.Remove(CacheKeys.PerfilesPopulares(i));
            }
            _logger.LogDebug("Caché de perfiles populares invalidada");
        }
    }

}
