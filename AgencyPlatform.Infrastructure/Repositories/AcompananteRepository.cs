using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Busqueda;
using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using System.Linq.Expressions;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class AcompananteRepository : IAcompananteRepository
    {
        private readonly AgencyPlatformDbContext _context;

        private readonly ILogger<AcompananteRepository> _logger;

        public AcompananteRepository(AgencyPlatformDbContext context, ILogger<AcompananteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<acompanante>> GetAllAsync()
        {
            // Usa AsSplitQuery para mejorar el rendimiento con múltiples includes
            return await _context.acompanantes
                .AsSplitQuery()
                .Include(a => a.usuario)
                .Include(a => a.fotos)
                .Include(a => a.servicios)
                .Include(a => a.acompanante_categoria)
                    .ThenInclude(ac => ac.categoria)
                .ToListAsync();
        }
        public async Task AgregarCategoriaSinGuardarAsync(int acompananteId, int categoriaId)
        {
            var relacion = new acompanante_categoria
            {
                acompanante_id = acompananteId,
                categoria_id = categoriaId,
                created_at = DateTime.UtcNow
            };

            await _context.acompanante_categorias.AddAsync(relacion);
            // No llamar a SaveChanges aquí
        }
       



        public async Task ActualizarScoreActividadAsync(int acompananteId, long scoreActividad)
        {
            var acompanante = await _context.acompanantes.FindAsync(acompananteId);
            if (acompanante != null)
            {
                acompanante.score_actividad = scoreActividad;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<acompanante?> GetByIdAsync(int id)
        {
            return await _context.acompanantes
                .Include(a => a.usuario)
                .Include(a => a.fotos)
                .Include(a => a.servicios)
                .Include(a => a.acompanante_categoria)
                    .ThenInclude(ac => ac.categoria)
                .FirstOrDefaultAsync(a => a.id == id);
        }
        public async Task<List<acompanante>> GetAllPaginatedAsync(int skip, int take)
        {
            return await _context.acompanantes
                .Include(a => a.fotos)
                .Include(a => a.servicios)
                .Include(a => a.acompanante_categoria).ThenInclude(ac => ac.categoria)
                .Include(a => a.agencia)
                .Include(a => a.usuario)
                .OrderByDescending(a => a.id)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }


        public async Task<acompanante?> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.acompanantes
                .Include(a => a.usuario)
                .Include(a => a.fotos)
                .Include(a => a.servicios)
                .Include(a => a.acompanante_categoria)
                    .ThenInclude(ac => ac.categoria)
                .FirstOrDefaultAsync(a => a.usuario_id == usuarioId);
        }

        public async Task AddAsync(acompanante entity)
        {
            await _context.acompanantes.AddAsync(entity);
        }

        public async Task UpdateAsync(acompanante entity)
        {
            _context.acompanantes.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(acompanante entity)
        {
            _context.acompanantes.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<acompanante>> GetDestacadosAsync()
        {
            var acompanantes = await _context.acompanantes
                .Include(a => a.usuario)
                .Include(a => a.fotos)
                .Include(a => a.anuncios_destacados)
                .Where(a => a.anuncios_destacados.Any(ad => ad.esta_activo == true && ad.fecha_fin > DateTime.Now))
                .Include(a => a.acompanante_categoria)
                    .ThenInclude(ac => ac.categoria)
                .ToListAsync();

            // Filtrar manualmente los anuncios destacados
            foreach (var acompanante in acompanantes)
            {
                acompanante.anuncios_destacados = acompanante.anuncios_destacados
                    .Where(ad => ad.esta_activo == true)
                    .ToList();
            }

            return acompanantes;
        }

        public async Task<List<acompanante>> GetRecientesAsync(int cantidad)
        {
            return await _context.acompanantes
                .Include(a => a.usuario)
                .Include(a => a.fotos)
                .Include(a => a.acompanante_categoria)
                    .ThenInclude(ac => ac.categoria)
                .Where(a => a.esta_disponible == true)
                .OrderByDescending(a => a.created_at)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<acompanante>> GetPopularesAsync(int cantidad)
        {
            return await _context.acompanantes
                .Include(a => a.usuario)
                .Include(a => a.fotos)
                .Include(a => a.acompanante_categoria)
                    .ThenInclude(ac => ac.categoria)
                .Where(a => a.esta_disponible == true)
                .OrderByDescending(a => a.score_actividad)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<acompanante>> BuscarAsync(string? busqueda, string? ciudad, string? pais, string? genero,
      int? edadMinima, int? edadMaxima, decimal? tarifaMinima, decimal? tarifaMaxima,
      bool? soloVerificados, bool? soloDisponibles, List<int>? categoriaIds,
      string? ordenarPor, int pagina, int elementosPorPagina)
        {
            // Consulta principal - mantiene la lógica original
            var query = _context.acompanantes
                .Include(a => a.usuario)
                .Include(a => a.fotos)
                .Include(a => a.servicios)
                .Include(a => a.acompanante_categoria)
                    .ThenInclude(ac => ac.categoria)
                .AsQueryable();

            // Vamos a guardar varios niveles de consulta para usar como fallbacks
            var queryNivel1 = query; // Sin filtros

            // Aplicar filtros como antes
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(a =>
                    (a.nombre_perfil != null && a.nombre_perfil.Contains(busqueda)) ||
                    (a.descripcion != null && a.descripcion.Contains(busqueda)) ||
                    (a.ciudad != null && a.ciudad.Contains(busqueda)) ||
                    (a.pais != null && a.pais.Contains(busqueda))
                );
            }

            var queryNivel2 = query; // Solo con búsqueda textual

            // Aplicar filtros de ubicación
            if (!string.IsNullOrWhiteSpace(ciudad))
            {
                query = query.Where(a => a.ciudad != null &&
                    EF.Functions.ILike(a.ciudad, $"%{ciudad}%"));
            }

            if (!string.IsNullOrWhiteSpace(pais))
            {
                query = query.Where(a => a.pais != null &&
                    (EF.Functions.ILike(a.pais, $"%{pais}%") ||
                     a.pais == "RD" && pais.Contains("Dominican")));
            }

            var queryNivel3 = query; // Con búsqueda textual y ubicación

            // Aplicar filtros demográficos
            if (!string.IsNullOrWhiteSpace(genero))
            {
                query = query.Where(a => a.genero != null &&
                    (EF.Functions.ILike(a.genero, $"%{genero}%") ||
                     a.genero == "M" && genero.Contains("masculino") ||
                     a.genero == "F" && genero.Contains("femenino")));
            }

            if (edadMinima.HasValue)
            {
                query = query.Where(a => a.edad >= edadMinima.Value);
            }

            if (edadMaxima.HasValue)
            {
                query = query.Where(a => a.edad <= edadMaxima.Value);
            }

            var queryNivel4 = query; // Con búsqueda, ubicación y demografía

            // Aplicar filtros económicos
            if (tarifaMinima.HasValue)
            {
                query = query.Where(a => a.tarifa_base >= tarifaMinima.Value);
            }

            if (tarifaMaxima.HasValue)
            {
                query = query.Where(a => a.tarifa_base <= tarifaMaxima.Value);
            }

            var queryNivel5 = query; // Con todos los filtros numéricos

            // Aplicar filtros booleanos
            if (soloVerificados == true)
            {
                query = query.Where(a => a.esta_verificado == true);
            }

            if (soloDisponibles == true)
            {
                query = query.Where(a => a.esta_disponible == true);
            }

            var queryNivel6 = query; // Con todos los filtros excepto categorías

            // Aplicar filtros de categoría
            if (categoriaIds != null && categoriaIds.Any())
            {
                query = query.Where(a => a.acompanante_categoria.Any(ac => categoriaIds.Contains(ac.categoria_id)));
            }

            // Aplicar ordenamiento estándar
            query = ordenarPor?.ToLower() switch
            {
                "precio_asc" => query.OrderBy(a => a.tarifa_base),
                "precio_desc" => query.OrderByDescending(a => a.tarifa_base),
                "edad_asc" => query.OrderBy(a => a.edad),
                "edad_desc" => query.OrderByDescending(a => a.edad),
                "popularidad" => query.OrderByDescending(a => a.score_actividad),
                _ => query.OrderByDescending(a => a.created_at)
            };

            // Aplicar paginación a la consulta principal
            var paginatedQuery = query.Skip((pagina - 1) * elementosPorPagina).Take(elementosPorPagina);

            // ALGORITMO DE SUGERENCIAS ALTERNATIVAS
            // Intentar con la consulta principal primero
            var resultados = await paginatedQuery.ToListAsync();

            // Si no hay resultados, intentar progresivamente con consultas menos restrictivas
            if (!resultados.Any())
            {
                // Nivel 6: Sin categorías
                resultados = await queryNivel6
                    .OrderByDescending(a => a.created_at)
                    .Take(elementosPorPagina)
                    .ToListAsync();

                if (!resultados.Any())
                {
                    // Nivel 5: Sin filtros booleanos
                    resultados = await queryNivel5
                        .OrderByDescending(a => a.created_at)
                        .Take(elementosPorPagina)
                        .ToListAsync();

                    if (!resultados.Any())
                    {
                        // Nivel 4: Sin filtros económicos
                        resultados = await queryNivel4
                            .OrderByDescending(a => a.created_at)
                            .Take(elementosPorPagina)
                            .ToListAsync();

                        if (!resultados.Any())
                        { 
                            // Nivel 3: Sin filtros demográficos
                            resultados = await queryNivel3
                                .OrderByDescending(a => a.created_at)
                                .Take(elementosPorPagina)
                                .ToListAsync();

                            if (!resultados.Any())
                            {
                                // Nivel 2: Solo con búsqueda textual
                                resultados = await queryNivel2
                                    .OrderByDescending(a => a.created_at)
                                    .Take(elementosPorPagina)
                                    .ToListAsync();

                                if (!resultados.Any())
                                {
                                    // Nivel 1: Sin filtros, mostrar los más recientes
                                    resultados = await queryNivel1
                                        .OrderByDescending(a => a.created_at)
                                        .Take(elementosPorPagina)
                                        .ToListAsync();
                                }
                            }
                        }
                    }
                }
            }

            return resultados;
        }

        public async Task<List<acompanante_categoria>> GetCategoriasByAcompananteIdAsync(int acompananteId)
        {
            return await _context.acompanante_categorias
                .Include(ac => ac.categoria)
                .Where(ac => ac.acompanante_id == acompananteId)
                .ToListAsync();
        }

        public async Task<bool> TieneCategoriaAsync(int acompananteId, int categoriaId)
        {
            return await _context.acompanante_categorias
                .AnyAsync(ac => ac.acompanante_id == acompananteId && ac.categoria_id == categoriaId);
        }

        public async Task AgregarCategoriaAsync(int acompananteId, int categoriaId)
        {
            // Verificar si la relación ya existe
            if (!await TieneCategoriaAsync(acompananteId, categoriaId))
            {
                // Crear nueva relación
                var nuevaCategoria = new acompanante_categoria
                {
                    acompanante_id = acompananteId,
                    categoria_id = categoriaId,
                    created_at = DateTime.UtcNow
                };

                await _context.acompanante_categorias.AddAsync(nuevaCategoria);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> TieneAcompanantesAsync(int categoriaId)
        {
            return await _context.acompanante_categorias
                .AnyAsync(ac => ac.categoria_id == categoriaId);
        }

        public async Task EliminarCategoriaAsync(int acompananteId, int categoriaId)
        {
            var relacion = await _context.acompanante_categorias
                .FirstOrDefaultAsync(ac => ac.acompanante_id == acompananteId && ac.categoria_id == categoriaId);

            if (relacion != null)
            {
                _context.acompanante_categorias.Remove(relacion);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<PerfilEstadisticasDto?> GetEstadisticasPerfilAsync(int acompananteId)
        {
            var perfil = await _context.vw_ranking_perfiles
                .Where(p => p.id == acompananteId)
                .Select(p => new PerfilEstadisticasDto
                {
                    AcompananteId = p.id ?? 0,
                    NombrePerfil = p.nombre_perfil,
                    Ciudad = p.ciudad,
                    EstaVerificado = p.esta_verificado ?? false,
                    TotalVisitas = p.total_visitas.HasValue ? Convert.ToInt32(p.total_visitas.Value) : 0,
                    TotalContactos = p.total_contactos.HasValue ? Convert.ToInt32(p.total_contactos.Value) : 0,
                    ScoreActividad = p.score_actividad.HasValue ? Convert.ToInt32(p.score_actividad.Value) : 0
                })
                .FirstOrDefaultAsync();

            if (perfil == null)
                return null;

            perfil.FotoUrl = await _context.fotos
                .Where(f => f.acompanante_id == acompananteId && f.es_principal == true)
                .Select(f => f.url)
                .FirstOrDefaultAsync();

            perfil.UltimaVisita = await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId)
                .OrderByDescending(v => v.fecha_visita)
                .Select(v => (DateTime?)v.fecha_visita)
                .FirstOrDefaultAsync();

            return perfil;
        }
        public async Task<int> CountByAgenciaIdAsync(int agenciaId)
        {
            return await _context.acompanantes
                .Where(a => a.agencia_id == agenciaId)
                .CountAsync();
        }

        public async Task<int> CountVerificadosByAgenciaIdAsync(int agenciaId)
        {
            return await _context.acompanantes
                .Where(a => a.agencia_id == agenciaId && a.esta_verificado == true)
                .CountAsync();
        }

        public async Task<List<acompanante>> GetDestacadosByAgenciaIdAsync(int agenciaId, int limit = 5)
        {
            return await _context.acompanantes
                .Include(a => a.visitas_perfils)
                .Include(a => a.contactos)
                .Where(a => a.agencia_id == agenciaId && a.esta_disponible == true)
                .OrderByDescending(a => a.visitas_perfils.Count + (a.contactos.Count * 5))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<PaginatedResult<acompanante>> GetIndependientesAsync(
            int pageNumber,
            int pageSize,
            string filterBy,
            string sortBy,
            bool sortDesc)
        {
            // Consulta base para acompañantes sin agencia
            var query = _context.acompanantes
                .Include(a => a.fotos)
                .Where(a => a.agencia_id == null && (a.esta_disponible ?? true))
                .AsQueryable();

            // Aplicar filtros si existen
            if (!string.IsNullOrEmpty(filterBy))
            {
                query = query.Where(a =>
                    a.nombre_perfil.Contains(filterBy) ||
                    a.ciudad.Contains(filterBy) ||
                    a.descripcion.Contains(filterBy));
            }

            // Aplicar ordenamiento
            query = ApplySorting(query, sortBy, sortDesc);

            // Obtener total de registros
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Aplicar paginación
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<acompanante>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }
        private IQueryable<acompanante> ApplySorting(
                IQueryable<acompanante> query,
                string sortBy,
                bool sortDesc)
        {
            switch (sortBy?.ToLower())
            {
                case "nombreperfil":
                    return sortDesc
                        ? query.OrderByDescending(a => a.nombre_perfil)
                        : query.OrderBy(a => a.nombre_perfil);
                case "edad":
                    return sortDesc
                        ? query.OrderByDescending(a => a.edad)
                        : query.OrderBy(a => a.edad);
                case "ciudad":
                    return sortDesc
                        ? query.OrderByDescending(a => a.ciudad)
                        : query.OrderBy(a => a.ciudad);
                case "tarifa":
                    return sortDesc
                        ? query.OrderByDescending(a => a.tarifa_base)
                        : query.OrderBy(a => a.tarifa_base);
                default:
                    return sortDesc
                        ? query.OrderByDescending(a => a.id)
                        : query.OrderBy(a => a.id);
            }





        }


        public async Task<List<acompanante>> GetMasVisitadosAsync(int cantidad)
        {
            // Obtenemos los acompañantes con más visitas
            var acompanantesIds = await _context.visitas_perfils
                .GroupBy(v => v.acompanante_id)
                .Select(g => new { AcompananteId = g.Key, Visitas = g.Count() })
                .OrderByDescending(a => a.Visitas)
                .Take(cantidad)
                .Select(a => a.AcompananteId)
                .ToListAsync();

            // Cargamos los datos completos de esos acompañantes
            var acompanantes = new List<acompanante>();
            foreach (var id in acompanantesIds)
            {
                // Aquí está el error - id ya es el valor entero, no un nullable
                var acompanante = await GetByIdAsync(id);
                if (acompanante != null && acompanante.esta_disponible == true)
                {
                    acompanantes.Add(acompanante);
                }
            }

            return acompanantes;
        }
        public async Task<List<acompanante>> GetByCategoriasAsync(List<int> categoriaIds, int cantidad)
        {
            return await _context.acompanante_categorias
                .Where(ac => categoriaIds.Contains(ac.categoria_id))
                .GroupBy(ac => ac.acompanante_id)
                .Select(g => new { AcompananteId = g.Key, CategoriaCount = g.Count() })
                .OrderByDescending(a => a.CategoriaCount)
                .Take(cantidad)
                .Select(a => a.AcompananteId)
                .Join(_context.acompanantes,
                    id => id,
                    a => a.id,
                    (id, a) => a)
                .Include(a => a.fotos)
                .Where(a => a.esta_disponible == true)
                .ToListAsync();
        }

        public async Task<List<acompanante>> GetByCiudadesAsync(List<string> ciudades, int cantidad)
        {
            return await _context.acompanantes
                .Include(a => a.fotos)
                .Where(a => ciudades.Contains(a.ciudad) && a.esta_disponible == true)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<int>> GetCategoriasByAcompananteIdAsync2(int acompananteId)
        {
            var categorias = await GetCategoriasByAcompananteIdAsync(acompananteId);
            return categorias.Select(c => c.categoria_id).ToList();
        }

        public async Task<bool> TieneCategoriasAsync(int acompananteId, List<int> categoriaIds)
        {
            var categorias = await GetCategoriasByAcompananteIdAsync(acompananteId);
            return categorias.Any(c => categoriaIds.Contains(c.categoria_id));
        }
        public async Task<List<int>> GetPerfilesVisitadosRecientementeIdsByClienteAsync(int clienteId, int cantidad)
        {
            return await _context.visitas_perfils
                .Where(v => v.cliente_id == clienteId)
                .OrderByDescending(v => v.fecha_visita)
                .Select(v => v.acompanante_id)
                .Distinct()
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<acompanante>> GetByIdsAsync(List<int> ids)
        {
            return await _context.acompanantes
                .Include(a => a.fotos)
                .Where(a => ids.Contains(a.id) && a.esta_disponible == true)
                .ToListAsync();
        }
        public async Task<Dictionary<int, AcompananteEstadisticas>> GetEstadisticasMultiplesAsync(List<int> ids)
        {
            // Obtener conteo de visitas en batch
            var visitas = await _context.visitas_perfils
                .Where(v => ids.Contains(v.acompanante_id))
                .GroupBy(v => v.acompanante_id)
                .Select(g => new { AcompananteId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Obtener conteo de contactos en batch
            var contactos = await _context.contactos
                .Where(c => ids.Contains(c.acompanante_id))
                .GroupBy(c => c.acompanante_id)
                .Select(g => new { AcompananteId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Obtener fotos principales en batch
            var fotos = await _context.fotos
                .Where(f => ids.Contains(f.acompanante_id) && f.es_principal == true)
                .Select(f => new { AcompananteId = f.acompanante_id, Url = f.url })
                .ToListAsync();

            // Alternativa para fotos si no hay principales
            var fotosAlternativas = await _context.fotos
                .Where(f => ids.Contains(f.acompanante_id))
                .GroupBy(f => f.acompanante_id)
                .Select(g => new { AcompananteId = g.Key, Url = g.FirstOrDefault().url })
                .ToListAsync();

            // Construir el diccionario de resultados
            var resultado = new Dictionary<int, AcompananteEstadisticas>();
            foreach (var id in ids)
            {
                resultado[id] = new AcompananteEstadisticas
                {
                    TotalVisitas = visitas.FirstOrDefault(v => v.AcompananteId == id)?.Count ?? 0,
                    TotalContactos = contactos.FirstOrDefault(c => c.AcompananteId == id)?.Count ?? 0,
                    FotoPrincipalUrl = fotos.FirstOrDefault(f => f.AcompananteId == id)?.Url ??
                                      fotosAlternativas.FirstOrDefault(f => f.AcompananteId == id)?.Url
                };
            }

            return resultado;
        }


        public async Task<List<int>> GetPerfilesVisitadosIdsByClienteAsync(int clienteId)
        {
            // Similar al método GetPerfilesVisitadosRecientementeIdsByClienteAsync pero sin límite
            return await _context.visitas_perfils
                .Where(v => v.cliente_id == clienteId)
                .OrderByDescending(v => v.fecha_visita)
                .Select(v => v.acompanante_id)
                .Distinct()
                .ToListAsync();
        }
        public async Task<List<int>> GetCategoriasDePerfilesAsync(List<int> perfilesIds)
        {
            return await _context.acompanante_categorias
                .Where(ac => perfilesIds.Contains(ac.acompanante_id))
                .Select(ac => ac.categoria_id)
                .Distinct()
                .ToListAsync();
        }
        public async Task<List<string>> GetCiudadesDePerfilesAsync(List<int> perfilesIds)
        {
            return await _context.acompanantes
                .Where(a => perfilesIds.Contains(a.id) && !string.IsNullOrEmpty(a.ciudad))
                .Select(a => a.ciudad)
                .Distinct()
                .ToListAsync();
        }
        public async Task<List<int>> GetIdsByCategoriasAsync(List<int> categoriaIds, int cantidad, List<int> excluirIds)
        {
            return await _context.acompanante_categorias
                .Where(ac => categoriaIds.Contains(ac.categoria_id) &&
                       !excluirIds.Contains(ac.acompanante_id))
                .GroupBy(ac => ac.acompanante_id)
                .Select(g => new { AcompananteId = g.Key, CategoriaCount = g.Count() })
                .OrderByDescending(a => a.CategoriaCount)
                .Take(cantidad)
                .Select(a => a.AcompananteId)
                .ToListAsync();
        }
        public async Task<List<int>> GetIdsByCiudadesAsync(List<string> ciudades, int cantidad, List<int> excluirIds)
        {
            return await _context.acompanantes
                .Where(a => ciudades.Contains(a.ciudad) &&
                       !excluirIds.Contains(a.id) &&
                       a.esta_disponible == true)
                .OrderByDescending(a => a.score_actividad)
                .Take(cantidad)
                .Select(a => a.id)
                .ToListAsync();
        }
        public async Task<List<int>> GetIdsPopularesAsync(int cantidad, List<int> excluirIds = null)
        {
            var query = _context.acompanantes
                .Where(a => a.esta_disponible == true);

            if (excluirIds != null && excluirIds.Any())
                query = query.Where(a => !excluirIds.Contains(a.id));

            return await query
                .OrderByDescending(a => a.score_actividad)
                .Take(cantidad)
                .Select(a => a.id)
                .ToListAsync();
        }

        public async Task<List<int>> GetCategoriasIdsDePerfilAsync(int perfilId)
        {
            return await _context.acompanante_categorias
                .Where(ac => ac.acompanante_id == perfilId)
                .Select(ac => ac.categoria_id)
                .ToListAsync();
        }
        public async Task<int> CountAsync(Func<acompanante, bool> predicate)
        {
            return await Task.FromResult(_context.acompanantes.Count(predicate));
        }
        public async Task<int> CountDestacadosAsync()
        {
            // Asumiendo que tienes una vista o consulta para destacados
            return await _context.anuncios_destacados
                               .Where(a => a.esta_activo == true &&
                                         a.acompanante.esta_disponible == true &&
                                         a.fecha_fin > DateTime.UtcNow)
                               .Select(a => a.acompanante_id)
                               .Distinct()
                               .CountAsync();
        }
       
        public async Task<List<acompanante>> GetRecientesPaginadosAsync(int skip, int take)
        {
            return await _context.acompanantes
                                .Where(a => a.esta_disponible == true)
                                .OrderByDescending(a => a.created_at)
                                .Skip(skip)
                                .Take(take)
                                .Include(a => a.fotos.Where(f => f.es_principal == true).Take(1))
                                .ToListAsync();
        }
        public async Task<List<acompanante>> GetAllPaginatedAsync(int skip, int take, Expression<Func<acompanante, bool>> predicate)
        {
            return await _context.acompanantes
                .Include(a => a.fotos)
                .Include(a => a.servicios)
                .Include(a => a.acompanante_categoria).ThenInclude(ac => ac.categoria)
                .Include(a => a.agencia)
                .Include(a => a.usuario)
                .Where(predicate) // Aplicar el filtro
                .OrderByDescending(a => a.id)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        public async Task<List<acompanante>> GetPopularesPaginadosAsync(int skip, int take)
        {
            return await _context.acompanantes
                                .Where(a => a.esta_disponible == true)
                                .OrderByDescending(a => a.score_actividad)
                                .Skip(skip)
                                .Take(take)
                                .Include(a => a.fotos.Where(f => f.es_principal == true).Take(1))
                                .ToListAsync();
        }
        public async Task<List<acompanante>> GetDestacadosPaginadosAsync(int skip, int take)
        {
            var destacadosIds = await _context.anuncios_destacados
                                            .Where(a => a.esta_activo == true &&
                                                      a.fecha_fin > DateTime.UtcNow)
                                            .OrderByDescending(a => a.fecha_inicio)
                                            .Select(a => a.acompanante_id)
                                            .Distinct()
                                            .Skip(skip)
                                            .Take(take)
                                            .ToListAsync();

            if (!destacadosIds.Any())
                return new List<acompanante>();

            return await _context.acompanantes
                                .Where(a => destacadosIds.Contains(a.id) &&
                                          a.esta_disponible == true)
                                .Include(a => a.fotos.Where(f => f.es_principal == true).Take(1))
                                .ToListAsync();
        }

        public async Task<List<acompanante>> SearchAdvancedAsync(
             string? searchText = null,
             bool matchExactPhrase = false,
             string? ciudad = null,
             string? pais = null,
             double? latitud = null,
             double? longitud = null,
             int? radioKm = null,
             string? genero = null,
             int? edadMinima = null,
             int? edadMaxima = null,
             int? alturaMinima = null,
             int? alturaMaxima = null,
             decimal? tarifaMinima = null,
             decimal? tarifaMaxima = null,
             string? moneda = null,
             List<int>? categoriaIds = null,
             List<int>? servicioIds = null,
             List<string>? idiomasRequeridos = null,
             bool soloVerificados = false,
             bool soloDisponibles = true,
             bool? conAgencia = null,
             int? agenciaId = null,
             bool soloConFotos = false,
             int? minimoFotos = null,
             string orderBy = "Relevancia",
             bool orderDescending = true,
             int pageNumber = 1,
             int pageSize = 20)
        {
            try
            {
                var query = _context.acompanantes
                    .AsSplitQuery() // Usar consultas divididas para mejor rendimiento con relaciones múltiples
                    .Include(a => a.fotos)
                    .Include(a => a.servicios)
                    .Include(a => a.acompanante_categoria)
                        .ThenInclude(ac => ac.categoria)
                    .Include(a => a.agencia)
                    .AsQueryable();

                _logger.LogDebug("Iniciando búsqueda avanzada con criterios: {SearchText}, {Ciudad}, {Pais}",
                    searchText, ciudad, pais);

                // Filtros principales
                if (soloDisponibles)
                    query = query.Where(a => a.esta_disponible == true);

                if (soloVerificados)
                    query = query.Where(a => a.esta_verificado == true);

                // Búsqueda por texto
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    if (matchExactPhrase)
                    {
                        query = query.Where(a =>
                            a.nombre_perfil.Contains(searchText) ||
                            a.descripcion.Contains(searchText) ||
                            a.ciudad.Contains(searchText) ||
                            a.pais.Contains(searchText) ||
                            a.idiomas.Contains(searchText));
                    }
                    else
                    {
                        // Separar términos para búsqueda parcial
                        var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var term in terms)
                        {
                            query = query.Where(a =>
                                a.nombre_perfil.Contains(term) ||
                                a.descripcion.Contains(term) ||
                                a.ciudad.Contains(term) ||
                                a.pais.Contains(term) ||
                                a.idiomas.Contains(term));
                        }
                    }
                }

                // Filtros de ubicación
                if (!string.IsNullOrWhiteSpace(ciudad))
                    query = query.Where(a => a.ciudad.ToLower() == ciudad.ToLower());

                if (!string.IsNullOrWhiteSpace(pais))
                    query = query.Where(a => a.pais.ToLower() == pais.ToLower());

                // Filtro por coordenadas y radio
                if (latitud.HasValue && longitud.HasValue && radioKm.HasValue)
                {
                    // Convertir radio de km a metros
                    var radioMetros = radioKm.Value * 1000;

                    // Usamos FromSqlRaw para ejecutar SQL nativo de PostgreSQL con earthdistance
                    var acompananteIds = _context.acompanantes
                        .Where(a => a.latitud != null && a.longitud != null)
                        .Select(a => new { a.id, a.latitud, a.longitud })
                        .AsEnumerable() // Traemos los datos a memoria para el cálculo
                        .Where(a =>
                            CalcularDistanciaHaversine(
                                latitud.Value,
                                longitud.Value,
                                a.latitud.Value,
                                a.longitud.Value) <= radioKm.Value)
                        .Select(a => a.id)
                        .ToList();

                    // Ahora aplicamos el filtro de IDs a la consulta original
                    query = query.Where(a => acompananteIds.Contains(a.id));
                }

                // Filtros de características
                if (!string.IsNullOrWhiteSpace(genero))
                    query = query.Where(a => a.genero.ToLower() == genero.ToLower());

                if (edadMinima.HasValue)
                    query = query.Where(a => a.edad >= edadMinima.Value);

                if (edadMaxima.HasValue)
                    query = query.Where(a => a.edad <= edadMaxima.Value);

                if (alturaMinima.HasValue)
                    query = query.Where(a => a.altura >= alturaMinima.Value);

                if (alturaMaxima.HasValue)
                    query = query.Where(a => a.altura <= alturaMaxima.Value);

                // Filtros de tarifa
                if (tarifaMinima.HasValue)
                    query = query.Where(a => a.tarifa_base >= tarifaMinima.Value);

                if (tarifaMaxima.HasValue)
                    query = query.Where(a => a.tarifa_base <= tarifaMaxima.Value);

                if (!string.IsNullOrWhiteSpace(moneda))
                    query = query.Where(a => a.moneda == moneda);

                // Filtro por categorías
                if (categoriaIds != null && categoriaIds.Any())
                {
                    query = query.Where(a => a.acompanante_categoria
                        .Any(ac => categoriaIds.Contains(ac.categoria_id)));
                }

                // Filtro por servicios
                if (servicioIds != null && servicioIds.Any())
                {
                    query = query.Where(a => a.servicios
                        .Any(s => servicioIds.Contains(s.id)));
                }

                // Filtro por idiomas
                if (idiomasRequeridos != null && idiomasRequeridos.Any())
                {
                    foreach (var idioma in idiomasRequeridos)
                    {
                        query = query.Where(a => a.idiomas.Contains(idioma));
                    }
                }

                // Filtros de agencia
                if (conAgencia.HasValue)
                {
                    if (conAgencia.Value)
                        query = query.Where(a => a.agencia_id != null);
                    else
                        query = query.Where(a => a.agencia_id == null);
                }

                if (agenciaId.HasValue)
                    query = query.Where(a => a.agencia_id == agenciaId.Value);

                // Filtros de fotos
                if (soloConFotos)
                    query = query.Where(a => a.fotos.Any());

                if (minimoFotos.HasValue && minimoFotos.Value > 0)
                    query = query.Where(a => a.fotos.Count >= minimoFotos.Value);

                // Ordenamiento
                switch (orderBy.ToLower())
                {
                    case "relevancia":
                        // Por defecto cuando hay búsqueda de texto
                        if (!string.IsNullOrWhiteSpace(searchText))
                        {
                            // La lógica de relevancia se aplica en la memoria después de filtrar
                            break;
                        }

                        // Si no hay texto de búsqueda, ordenar por score de actividad
                        query = orderDescending
                            ? query.OrderByDescending(a => a.score_actividad)
                            : query.OrderBy(a => a.score_actividad);
                        break;

                    case "fecha":
                    case "recientes":
                        query = orderDescending
                            ? query.OrderByDescending(a => a.created_at)
                            : query.OrderBy(a => a.created_at);
                        break;

                    case "tarifa":
                    case "tarifabase":
                    case "precio":
                        query = orderDescending
                            ? query.OrderByDescending(a => a.tarifa_base)
                            : query.OrderBy(a => a.tarifa_base);
                        break;

                    case "nombre":
                        query = orderDescending
                            ? query.OrderByDescending(a => a.nombre_perfil)
                            : query.OrderBy(a => a.nombre_perfil);
                        break;

                    case "edad":
                        query = orderDescending
                            ? query.OrderByDescending(a => a.edad)
                            : query.OrderBy(a => a.edad);
                        break;

                    case "popularidad":
                    case "visitas":
                        // Se ordena por score_actividad para reflejar popularidad
                        query = orderDescending
                            ? query.OrderByDescending(a => a.score_actividad)
                            : query.OrderBy(a => a.score_actividad);
                        break;

                    default:
                        // Orden por defecto: popularidad descendente
                        query = query.OrderByDescending(a => a.score_actividad);
                        break;
                }

                // Aplicar paginación
                var skip = (pageNumber - 1) * pageSize;

                // Ejecutar consulta y loguear resultados para depuración
                var acompanantes = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Búsqueda devolvió {Count} acompañantes", acompanantes.Count);

                // Verificar que las relaciones estén cargadas
                foreach (var a in acompanantes)
                {
                    _logger.LogDebug("Acompañante ID: {Id}, Nombre: {Nombre}, Fotos: {FotoCount}, Categorías: {CatCount}",
                        a.id,
                        a.nombre_perfil,
                        a.fotos?.Count ?? 0,
                        a.acompanante_categoria?.Count ?? 0);
                }

                // Aplicar relevancia personalizada para búsqueda de texto (si es necesario)
                if (!string.IsNullOrWhiteSpace(searchText) && orderBy.ToLower() == "relevancia")
                {
                    // Cálculo de relevancia basado en coincidencias y posición
                    var ordenados = acompanantes.Select(a => new
                    {
                        Acompanante = a,
                        Relevancia = CalcularRelevancia(a, searchText)
                    })
                    .OrderByDescending(item => item.Relevancia)
                    .Select(item => item.Acompanante);

                    return orderDescending ? ordenados.ToList() : ordenados.Reverse().ToList();
                }

                return acompanantes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar búsqueda avanzada");
                throw;
            }
        }


        // IMPORTANTE: Aplicar exactamente los mismos filtros que en SearchAdvancedAsync
        public async Task<int> CountSearchAdvancedAsync(
            string? searchText = null,
            bool matchExactPhrase = false,
            string? ciudad = null,
            string? pais = null,
            double? latitud = null,
            double? longitud = null,
            int? radioKm = null,
            string? genero = null,
            int? edadMinima = null,
            int? edadMaxima = null,
            int? alturaMinima = null,
            int? alturaMaxima = null,
            decimal? tarifaMinima = null,
            decimal? tarifaMaxima = null,
            string? moneda = null,
            List<int>? categoriaIds = null,
            List<int>? servicioIds = null,
            List<string>? idiomasRequeridos = null,
            bool soloVerificados = false,
            bool soloDisponibles = true,
            bool? conAgencia = null,
            int? agenciaId = null,
            bool soloConFotos = false,
            int? minimoFotos = null)
        {
            try
            {
                var query = _context.acompanantes.AsQueryable();

                // IMPORTANTE: Aplicar exactamente los mismos filtros que en SearchAdvancedAsync

                // Filtros principales
                if (soloDisponibles)
                    query = query.Where(a => a.esta_disponible == true);

                if (soloVerificados)
                    query = query.Where(a => a.esta_verificado == true);

                // Búsqueda por texto mejorada con ILIKE para insensibilidad a mayúsculas/minúsculas
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    if (matchExactPhrase)
                    {
                        // Búsqueda de frase exacta con ILIKE
                        query = query.Where(a =>
                            EF.Functions.ILike(a.nombre_perfil ?? "", $"%{searchText}%") ||
                            EF.Functions.ILike(a.descripcion ?? "", $"%{searchText}%") ||
                            EF.Functions.ILike(a.ciudad ?? "", $"%{searchText}%") ||
                            EF.Functions.ILike(a.pais ?? "", $"%{searchText}%") ||
                            EF.Functions.ILike(a.idiomas ?? "", $"%{searchText}%"));
                    }
                    else
                    {
                        // Búsqueda de términos individuales más flexible
                        var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var term in terms)
                        {
                            var searchTerm = term.Trim();
                            query = query.Where(a =>
                                EF.Functions.ILike(a.nombre_perfil ?? "", $"%{searchTerm}%") ||
                                EF.Functions.ILike(a.descripcion ?? "", $"%{searchTerm}%") ||
                                EF.Functions.ILike(a.ciudad ?? "", $"%{searchTerm}%") ||
                                EF.Functions.ILike(a.pais ?? "", $"%{searchTerm}%") ||
                                EF.Functions.ILike(a.idiomas ?? "", $"%{searchTerm}%"));
                        }
                    }
                }

                // Filtros de ubicación más flexibles
                if (!string.IsNullOrWhiteSpace(ciudad))
                {
                    // Búsqueda de coincidencia parcial para ciudad
                    query = query.Where(a => EF.Functions.ILike(a.ciudad ?? "", $"%{ciudad}%"));
                }

                if (!string.IsNullOrWhiteSpace(pais))
                {
                    // Búsqueda de coincidencia parcial para país
                    query = query.Where(a => EF.Functions.ILike(a.pais ?? "", $"%{pais}%"));
                }

                // Filtro por coordenadas y radio
                if (latitud.HasValue && longitud.HasValue)
                {
                    // Código de proximidad aquí si es necesario
                }

                // Filtros de características con búsqueda más flexible para género
                if (!string.IsNullOrWhiteSpace(genero))
                {
                    // Normalizar búsqueda de género para aceptar variaciones
                    var generoLower = genero.ToLower().Trim();
                    query = query.Where(a =>
                        EF.Functions.ILike(a.genero ?? "", $"%{generoLower}%") ||
                        (generoLower == "masculino" && (
                            EF.Functions.ILike(a.genero ?? "", "%m%") ||
                            EF.Functions.ILike(a.genero ?? "", "%hombre%") ||
                            EF.Functions.ILike(a.genero ?? "", "%varon%")
                        )) ||
                        (generoLower == "femenino" && (
                            EF.Functions.ILike(a.genero ?? "", "%f%") ||
                            EF.Functions.ILike(a.genero ?? "", "%mujer%") ||
                            EF.Functions.ILike(a.genero ?? "", "%chica%")
                        ))
                    );
                }

                // Filtros numéricos - mantener rangos
                if (edadMinima.HasValue)
                    query = query.Where(a => a.edad >= edadMinima.Value);

                if (edadMaxima.HasValue)
                    query = query.Where(a => a.edad <= edadMaxima.Value);

                if (alturaMinima.HasValue)
                    query = query.Where(a => a.altura >= alturaMinima.Value);

                if (alturaMaxima.HasValue)
                    query = query.Where(a => a.altura <= alturaMaxima.Value);

                // Filtros de tarifa
                if (tarifaMinima.HasValue)
                    query = query.Where(a => a.tarifa_base >= tarifaMinima.Value);

                if (tarifaMaxima.HasValue)
                    query = query.Where(a => a.tarifa_base <= tarifaMaxima.Value);

                if (!string.IsNullOrWhiteSpace(moneda))
                {
                    // Búsqueda más flexible para moneda
                    var monedaLower = moneda.ToLower();
                    query = query.Where(a =>
                        EF.Functions.ILike(a.moneda ?? "", $"%{monedaLower}%") ||
                        (monedaLower == "eur" && EF.Functions.ILike(a.moneda ?? "", "%euro%")) ||
                        (monedaLower == "usd" && EF.Functions.ILike(a.moneda ?? "", "%dolar%")) ||
                        (monedaLower == "dop" && EF.Functions.ILike(a.moneda ?? "", "%peso%")) ||
                        (monedaLower == "peso" && (
                            EF.Functions.ILike(a.moneda ?? "", "%peso%") ||
                            EF.Functions.ILike(a.moneda ?? "", "%dop%") ||
                            EF.Functions.ILike(a.moneda ?? "", "%rd%")
                        ))
                    );
                }

                // Filtro por categorías - mantener igual
                if (categoriaIds != null && categoriaIds.Any())
                {
                    query = query.Where(a => a.acompanante_categoria
                        .Any(ac => categoriaIds.Contains(ac.categoria_id)));
                }

                // Filtro por servicios - mantener igual
                if (servicioIds != null && servicioIds.Any())
                {
                    query = query.Where(a => a.servicios
                        .Any(s => servicioIds.Contains(s.id)));
                }

                // Mejora para búsqueda de idiomas
                if (idiomasRequeridos != null && idiomasRequeridos.Any())
                {
                    foreach (var idioma in idiomasRequeridos)
                    {
                        // Búsqueda más flexible para idiomas
                        var idiomaTermino = idioma.ToLower().Trim();
                        query = query.Where(a =>
                            EF.Functions.ILike(a.idiomas ?? "", $"%{idiomaTermino}%") ||
                            // Manejar variaciones comunes
                            (idiomaTermino == "español" && EF.Functions.ILike(a.idiomas ?? "", "%espa%")) ||
                            (idiomaTermino == "inglés" && (
                                EF.Functions.ILike(a.idiomas ?? "", "%ingl%") ||
                                EF.Functions.ILike(a.idiomas ?? "", "%engl%")
                            )) ||
                            (idiomaTermino == "frances" && EF.Functions.ILike(a.idiomas ?? "", "%franc%"))
                        );
                    }
                }

                // Filtros de agencia
                if (conAgencia.HasValue)
                {
                    if (conAgencia.Value)
                        query = query.Where(a => a.agencia_id != null);
                    else
                        query = query.Where(a => a.agencia_id == null);
                }

                if (agenciaId.HasValue)
                    query = query.Where(a => a.agencia_id == agenciaId.Value);

                // Filtros de fotos
                if (soloConFotos)
                {
                    query = query.Where(a => a.fotos.Any());
                }

                if (minimoFotos.HasValue && minimoFotos.Value > 0)
                {
                    query = query.Where(a => a.fotos.Count >= minimoFotos.Value);
                }

                // Ejecutar el conteo
                var total = await query.CountAsync();
                _logger.LogInformation("Conteo de búsqueda: {Total} acompañantes", total);
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar resultados de búsqueda avanzada");
                throw;
            }
        }
        private double CalcularRelevancia(acompanante a, string searchText, AdvancedSearchCriteriaDto? criteria = null)
        {
            double relevancia = 0;

            // COINCIDENCIA DE TEXTO
            if (!string.IsNullOrEmpty(searchText))
            {
                var searchTerms = searchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Nombre del perfil - alta relevancia
                if (a.nombre_perfil != null)
                {
                    if (a.nombre_perfil.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                        relevancia += 100; // Coincidencia exacta del nombre completo
                    else
                    {
                        foreach (var term in searchTerms)
                        {
                            if (a.nombre_perfil.ToLower().Contains(term))
                                relevancia += 50; // Coincidencia parcial en nombre
                        }
                    }
                }

                // Descripción - relevancia media
                if (a.descripcion != null)
                {
                    var descripcionLower = a.descripcion.ToLower();
                    foreach (var term in searchTerms)
                    {
                        if (descripcionLower.Contains(term))
                            relevancia += 20;
                    }
                }

                // Ciudad/País - relevancia alta para ubicación
                if (a.ciudad != null && searchTerms.Any(term => a.ciudad.ToLower().Contains(term)))
                    relevancia += 80;

                if (a.pais != null && searchTerms.Any(term => a.pais.ToLower().Contains(term)))
                    relevancia += 70;
            }

            // BONIFICACIONES POR COINCIDENCIA CON CRITERIOS DE BÚSQUEDA
            if (criteria != null)
            {
                // Ubicación exacta tiene alta prioridad
                if (!string.IsNullOrEmpty(criteria.Ciudad) &&
                    !string.IsNullOrEmpty(a.ciudad) &&
                    a.ciudad.Equals(criteria.Ciudad, StringComparison.OrdinalIgnoreCase))
                    relevancia += 50;

                if (!string.IsNullOrEmpty(criteria.Pais) &&
                    !string.IsNullOrEmpty(a.pais) &&
                    a.pais.Equals(criteria.Pais, StringComparison.OrdinalIgnoreCase))
                    relevancia += 40;

                // Tarifa ideal (más cercana al punto medio del rango solicitado)
                if (criteria.TarifaMinima.HasValue && criteria.TarifaMaxima.HasValue && a.tarifa_base.HasValue)
                {
                    var tarifaMedia = (criteria.TarifaMinima.Value + criteria.TarifaMaxima.Value) / 2;
                    var diferencia = Math.Abs((decimal)a.tarifa_base.Value - tarifaMedia);
                    var rangoTotal = criteria.TarifaMaxima.Value - criteria.TarifaMinima.Value;

                    if (rangoTotal > 0)
                    {
                        // Calcular cuán lejos está del punto ideal (0 = justo en el medio, 1 = en el límite)
                        double distanciaRelativa = (double)(diferencia / (rangoTotal / 2));
                        // Bonificación inversamente proporcional a la distancia
                        double bonificacionTarifa = 30 * Math.Max(0, 1 - distanciaRelativa);
                        relevancia += bonificacionTarifa;
                    }
                }

                // Edad ideal (más cercana al punto medio del rango)
                if (criteria.EdadMinima.HasValue && criteria.EdadMaxima.HasValue && a.edad.HasValue)
                {
                    var edadMedia = (criteria.EdadMinima.Value + criteria.EdadMaxima.Value) / 2.0;
                    var diferenciaEdad = Math.Abs(a.edad.Value - edadMedia);
                    var rangoEdad = criteria.EdadMaxima.Value - criteria.EdadMinima.Value;

                    if (rangoEdad > 0)
                    {
                        double distanciaRelativaEdad = diferenciaEdad / (rangoEdad / 2.0);
                        double bonificacionEdad = 20 * Math.Max(0, 1 - distanciaRelativaEdad);
                        relevancia += bonificacionEdad;
                    }
                }

                // Bonificación por categorías coincidentes
                if (criteria.CategoriaIds != null && criteria.CategoriaIds.Any() && a.acompanante_categoria != null)
                {
                    var categoriasAcompanante = a.acompanante_categoria.Select(ac => ac.categoria_id).ToList();
                    int coincidencias = criteria.CategoriaIds.Count(c => categoriasAcompanante.Contains(c));

                    // Mayor bonificación si coinciden todas las categorías buscadas
                    if (coincidencias == criteria.CategoriaIds.Count)
                        relevancia += 50;
                    else
                        relevancia += coincidencias * 10; // Bonificación parcial por cada coincidencia
                }

                // Bonificación por idiomas requeridos
                if (criteria.IdiomasRequeridos != null && criteria.IdiomasRequeridos.Any() && !string.IsNullOrEmpty(a.idiomas))
                {
                    var idiomasLower = a.idiomas.ToLower();
                    int idiomasCoincidentes = criteria.IdiomasRequeridos.Count(i =>
                        idiomasLower.Contains(i.ToLower()) ||
                        (i.ToLower() == "español" && idiomasLower.Contains("espa")) ||
                        (i.ToLower() == "inglés" && (idiomasLower.Contains("ingl") || idiomasLower.Contains("engl")))
                    );

                    // Mayor bonificación si habla todos los idiomas requeridos
                    if (idiomasCoincidentes == criteria.IdiomasRequeridos.Count)
                        relevancia += 40;
                    else
                        relevancia += idiomasCoincidentes * 10;
                }
            }

            // FACTORES DE CALIDAD - SIEMPRE SE APLICAN

            // Bonificación por verificación
            if (a.esta_verificado == true)
                relevancia += 40;

            // Bonificación por actividad reciente
            relevancia += Math.Min(30, (a.score_actividad ?? 0) / 10.0);

            // Bonificación por cantidad de fotos
            if (a.fotos != null)
            {
                int numFotos = a.fotos.Count;
                relevancia += Math.Min(25, numFotos * 3); // Hasta 25 puntos por fotos
            }

            // Bonificación por perfil completo
            int camposCompletados = 0;
            if (!string.IsNullOrEmpty(a.nombre_perfil)) camposCompletados++;
            if (!string.IsNullOrEmpty(a.descripcion)) camposCompletados++;
            if (!string.IsNullOrEmpty(a.ciudad)) camposCompletados++;
            if (!string.IsNullOrEmpty(a.pais)) camposCompletados++;
            if (a.edad.HasValue) camposCompletados++;
            if (a.tarifa_base.HasValue) camposCompletados++;
            if (!string.IsNullOrEmpty(a.idiomas)) camposCompletados++;

            // Bonificación por completitud de perfil
            relevancia += camposCompletados * 5; // Hasta 35 puntos adicionales

            return relevancia;
        }        // Añade este método dentro de la clase AcompananteRepository
        public async Task<List<acompanante>> GetByProximityAsync(
            double latitud,
            double longitud,
            int radioKm,
            string orderBy = null,
            bool orderDescending = false,
            int? maxResults = null)
        {
            string sql = @"
        SELECT * FROM plataforma.acompanantes a 
        WHERE a.latitud IS NOT NULL AND a.longitud IS NOT NULL
        AND earth_distance(ll_to_earth(a.latitud, a.longitud), 
                          ll_to_earth(@latitud, @longitud)) <= @radioMetros";

            // Agregar ordenamiento
            if (orderBy?.ToLower() == "distancia")
            {
                sql += " ORDER BY earth_distance(ll_to_earth(a.latitud, a.longitud), ll_to_earth(@latitud, @longitud))";
                if (orderDescending)
                    sql += " DESC";
            }
            else if (!string.IsNullOrEmpty(orderBy))
            {
                // Manejar otros tipos de ordenamiento si es necesario
                sql += $" ORDER BY a.{orderBy}";
                if (orderDescending)
                    sql += " DESC";
            }
            else
            {
                // Ordenamiento predeterminado por distancia
                sql += " ORDER BY earth_distance(ll_to_earth(a.latitud, a.longitud), ll_to_earth(@latitud, @longitud))";
            }

            if (maxResults.HasValue)
            {
                sql += " LIMIT " + maxResults.Value;
            }

            var parameters = new[]
            {
        new Npgsql.NpgsqlParameter("@latitud", latitud),
        new Npgsql.NpgsqlParameter("@longitud", longitud),
        new Npgsql.NpgsqlParameter("@radioMetros", radioKm * 1000)
    };

            return await _context.acompanantes
                .FromSqlRaw(sql, parameters)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<List<dynamic>> GetCiudadesConCoincidenciaAsync(string searchTerm, int limit)
        {
            var result = await _context.acompanantes
                .Where(a => a.ciudad.Contains(searchTerm) && a.esta_disponible == true)
                .GroupBy(a => a.ciudad)
                .Select(g => new { nombre = g.Key, cantidad = g.Count() })
                .OrderByDescending(g => g.cantidad)
                .Take(limit)
                .ToListAsync();

            // Convertir el resultado a List<dynamic> si es necesario
            return result.Cast<dynamic>().ToList();
        }
        public async Task<List<acompanante>> GetAllActivosAsync()
        {
            return await _context.acompanantes
                .Where(a => a.esta_disponible == true)
                .Include(a => a.usuario)
                .Include(a => a.agencia)
                .ToListAsync();
        }
        public async Task<List<acompanante>> GetPerfilesInactivosDesdeAsync(DateTime fechaLimite)
        {
            // Obtener perfiles disponibles que no han tenido actividad desde fechaLimite
            var perfilesConActividad = await _context.acompanantes
                .Where(a => a.esta_disponible == true)
                .Include(a => a.usuario)
                .Include(a => a.visitas_perfils.Where(v => v.fecha_visita >= fechaLimite))
                .Include(a => a.contactos.Where(c => c.fecha_contacto >= fechaLimite))
                .AsSplitQuery() // Para evitar problemas con múltiples includes
                .ToListAsync();

            // Filtrar los perfiles que no tienen visitas ni contactos recientes
            return perfilesConActividad
                .Where(a => !a.visitas_perfils.Any() && !a.contactos.Any())
                .ToList();
        }
        public async Task<int> CountVisitasPeriodoAsync(int acompananteId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.visitas_perfils
                .CountAsync(v => v.acompanante_id == acompananteId &&
                               v.fecha_visita >= fechaInicio &&
                               v.fecha_visita < fechaFin);
        }

        public async Task<int> CountContactosPeriodoAsync(int acompananteId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.contactos
                .CountAsync(c => c.acompanante_id == acompananteId &&
                               c.fecha_contacto >= fechaInicio &&
                               c.fecha_contacto < fechaFin);
        }
        public async Task ActualizarVistaRankingPerfilesAsync()
        {
            try
            {
                // Verificar si las vistas existen como vistas normales
                var normalViewExistQuery = @"
            SELECT COUNT(*) 
            FROM pg_catalog.pg_views 
            WHERE schemaname = 'plataforma' AND viewname = 'vw_ranking_perfiles'";

                int normalViewCount = await ExecuteScalarQueryAsync<int>(normalViewExistQuery);
                bool isNormalView = normalViewCount > 0;

                _logger.LogInformation($"Estado de vistas - Vista Normal: {isNormalView}");

                if (isNormalView)
                {
                    // Las vistas ya existen como vistas normales
                    _logger.LogInformation("Las vistas son normales y se actualizan automáticamente con cada consulta. No es necesario hacer nada.");
                }
                else
                {
                    // Este código solo se ejecutaría si las vistas no existieran, lo cual no es tu caso
                    _logger.LogWarning("Las vistas no existen en la base de datos. Contacte al administrador del sistema para crearlas.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar las vistas: {Message}", ex.Message);
                throw;
            }
        }





        public async Task<acompanante> GetByStripeAccountIdAsync(string stripeAccountId)
        {
            return await _context.acompanantes
                .FirstOrDefaultAsync(a => a.stripe_account_id == stripeAccountId);
        }



      

        // Método auxiliar para ejecutar consultas que devuelven un único valor
        private async Task<T> ExecuteScalarQueryAsync<T>(string sql)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            if (command.Connection.State != System.Data.ConnectionState.Open)
                await command.Connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }
        private double CalcularDistanciaHaversine(double lat1, double lon1, double lat2, double lon2)
        {
            // Radio de la Tierra en kilómetros
            const double R = 6371;

            // Convertir de grados a radianes
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            // Fórmula de Haversine
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return distance;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
