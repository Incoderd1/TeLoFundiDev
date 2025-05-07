using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Busqueda;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services.AdvancedSearch;
using AgencyPlatform.Application;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using AgencyPlatform.Application.Interfaces.Services.Geocalizacion;
using AgencyPlatform.Infrastructure.Services.Geocalizacion;
using AgencyPlatform.Application.Interfaces.Services.Static;

namespace AgencyPlatform.Infrastructure.Services.AdvancedSearch
{
    public class AdvancedSearchService : IAdvancedSearchService
    {
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IVisitaPerfilRepository _visitaRepository;
        private readonly IContactoRepository _contactoRepository;
        private readonly IBusquedaRepository _busquedaRepository;
        private readonly IFotoRepository _fotoRepository;
        private readonly IAgenciaRepository _agenciaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdvancedSearchService> _logger;

        private readonly IGeocodingService _geocodingService;

        private const string CACHE_KEY_SUGGESTIONS = "SearchSuggestions";
        private readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

        public AdvancedSearchService(
            IAcompananteRepository acompananteRepository,
            ICategoriaRepository categoriaRepository,
            IVisitaPerfilRepository visitaRepository,
            IContactoRepository contactoRepository,
            IBusquedaRepository busquedaRepository,
            IFotoRepository fotoRepository,
            IAgenciaRepository agenciaRepository,
            IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        IMemoryCache cache,
            ILogger<AdvancedSearchService> logger, IGeocodingService geocodingService)
        {
            _acompananteRepository = acompananteRepository;
            _categoriaRepository = categoriaRepository;
            _visitaRepository = visitaRepository;
            _contactoRepository = contactoRepository;
            _busquedaRepository = busquedaRepository;
            _fotoRepository = fotoRepository;
            _agenciaRepository = agenciaRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;

            _geocodingService = geocodingService;
        }

        public async Task<PaginatedResultDto<AcompananteSearchResultDto>> SearchAcompanantesAsync(AdvancedSearchCriteriaDto criteria)
        {
            try
            {
                _logger.LogInformation("Realizando búsqueda avanzada con criterios: {Criteria}",
                    JsonSerializer.Serialize(criteria));

                // Validar y ajustar parámetros de paginación
                criteria.PageNumber = criteria.PageNumber < 1 ? 1 : criteria.PageNumber;
                criteria.PageSize = criteria.PageSize switch
                {
                    < 1 => 10,
                    > 50 => 50,
                    _ => criteria.PageSize
                };

                // Geocodificar automáticamente basado en Ciudad/País
                if (!string.IsNullOrWhiteSpace(criteria.Ciudad) || !string.IsNullOrWhiteSpace(criteria.Pais))
                {
                    string locationQuery = string.Join(", ",
                        new[] { criteria.Ciudad, criteria.Pais }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                    var coordinates = await _geocodingService.GeocodeAddressAsync(locationQuery);
                    if (coordinates.HasValue)
                    {
                        // Asignar automáticamente las coordenadas obtenidas
                        criteria.Latitud = coordinates.Value.latitude;
                        criteria.Longitud = coordinates.Value.longitude;
                        criteria.RadioKm = criteria.RadioKm ?? 10; // Radio predeterminado de 10 km si no se especifica

                        _logger.LogInformation($"Ubicación '{locationQuery}' geocodificada a: {criteria.Latitud}, {criteria.Longitud} con radio {criteria.RadioKm} km");
                    }
                    else
                    {
                        _logger.LogWarning($"No se pudieron obtener coordenadas para '{locationQuery}'");
                    }
                }

                // Realizar búsqueda en la base de datos
                var acompanantes = await _acompananteRepository.SearchAdvancedAsync(
                    searchText: criteria.SearchText,
                    matchExactPhrase: criteria.MatchExactPhrase ?? false,
                    ciudad: criteria.Ciudad,
                    pais: criteria.Pais,
                    latitud: criteria.Latitud,
                    longitud: criteria.Longitud,
                    radioKm: criteria.RadioKm,
                    genero: criteria.Genero,
                    edadMinima: criteria.EdadMinima,
                    edadMaxima: criteria.EdadMaxima,
                    alturaMinima: criteria.AlturaMinima,
                    alturaMaxima: criteria.AlturaMaxima,
                    tarifaMinima: criteria.TarifaMinima,
                    tarifaMaxima: criteria.TarifaMaxima,
                    moneda: criteria.Moneda,
                    categoriaIds: criteria.CategoriaIds,
                    servicioIds: criteria.ServicioIds,
                    idiomasRequeridos: criteria.IdiomasRequeridos,
                    soloVerificados: criteria.SoloVerificados ?? false,
                    soloDisponibles: criteria.SoloDisponibles ?? true,
                    conAgencia: criteria.ConAgencia,
                    agenciaId: criteria.AgenciaId,
                    soloConFotos: criteria.SoloConFotos ?? false,
                    minimoFotos: criteria.MinimoFotos,
                    orderBy: criteria.Latitud.HasValue && criteria.Longitud.HasValue ? "distancia" : criteria.OrderBy, // Ordenar por distancia si hay coordenadas
                    orderDescending: criteria.OrderDescending,
                    pageNumber: criteria.PageNumber,
                    pageSize: criteria.PageSize);

                _logger.LogInformation("Búsqueda devolvió {Count} acompañantes desde el repositorio", acompanantes.Count);

                // Obtener total de resultados
                int totalItems = await _acompananteRepository.CountSearchAdvancedAsync(
                    searchText: criteria.SearchText,
                    matchExactPhrase: criteria.MatchExactPhrase ?? false,
                    ciudad: criteria.Ciudad,
                    pais: criteria.Pais,
                    latitud: criteria.Latitud,
                    longitud: criteria.Longitud,
                    radioKm: criteria.RadioKm,
                    genero: criteria.Genero,
                    edadMinima: criteria.EdadMinima,
                    edadMaxima: criteria.EdadMaxima,
                    alturaMinima: criteria.AlturaMinima,
                    alturaMaxima: criteria.AlturaMaxima,
                    tarifaMinima: criteria.TarifaMinima,
                    tarifaMaxima: criteria.TarifaMaxima,
                    moneda: criteria.Moneda,
                    categoriaIds: criteria.CategoriaIds,
                    servicioIds: criteria.ServicioIds,
                    idiomasRequeridos: criteria.IdiomasRequeridos,
                    soloVerificados: criteria.SoloVerificados ?? false,
                    soloDisponibles: criteria.SoloDisponibles ?? true,
                    conAgencia: criteria.ConAgencia,
                    agenciaId: criteria.AgenciaId,
                    soloConFotos: criteria.SoloConFotos ?? false,
                    minimoFotos: criteria.MinimoFotos);

                _logger.LogInformation("Total de resultados: {Total}", totalItems);

                // *** INICIO - NUEVA FUNCIONALIDAD PARA RESULTADOS ALTERNATIVOS ***
                // Si no hay resultados, buscar alternativas con criterios más relajados
                if (totalItems == 0)
                {
                    _logger.LogInformation("No se encontraron resultados exactos, buscando alternativas");

                    // Crear criterios más relajados eliminando las restricciones más limitantes
                    var criteriosRelajados = new AdvancedSearchCriteriaDto
                    {
                        SearchText = criteria.SearchText,
                        // Eliminar filtros restrictivos geográficos pero mantener las coordenadas para búsqueda por distancia
                        Ciudad = null, // No filtrar por ciudad exacta
                        Pais = criteria.Pais, // Mantener país como filtro más general
                        Latitud = criteria.Latitud, // Mantener coordenadas para buscar por cercanía
                        Longitud = criteria.Longitud,
                        RadioKm = criteria.Latitud.HasValue && criteria.Longitud.HasValue
                            ? (criteria.RadioKm.HasValue ? criteria.RadioKm * 2 : 20) // Ampliar el radio de búsqueda
                            : null,

                        // Mantener filtros de características principales
                        Genero = criteria.Genero,
                        EdadMinima = criteria.EdadMinima,
                        EdadMaxima = criteria.EdadMaxima,

                        // Eliminar filtros secundarios
                        CategoriaIds = null, // No filtrar por categorías
                        ServicioIds = null, // No filtrar por servicios
                        IdiomasRequeridos = null, // No filtrar por idiomas
                        MinimoFotos = null, // No filtrar por número de fotos

                        // Relajar filtros de precio
                        TarifaMinima = criteria.TarifaMinima != null ? criteria.TarifaMinima * 0.8m : null,
                        TarifaMaxima = criteria.TarifaMaxima != null ? criteria.TarifaMaxima * 1.2m : null,
                        Moneda = criteria.Moneda,

                        // Mantener filtros de calidad básicos
                        SoloVerificados = criteria.SoloVerificados,
                        SoloDisponibles = criteria.SoloDisponibles,
                        SoloConFotos = criteria.SoloConFotos,

                        // Paginación modificada para mostrar menos resultados alternativos
                        PageNumber = 1,
                        PageSize = 5,

                        // Ordenar por relevancia o distancia si hay coordenadas
                        OrderBy = criteria.Latitud.HasValue && criteria.Longitud.HasValue ? "distancia" : "relevancia",
                        OrderDescending = false // Para distancia, queremos los más cercanos primero
                    };

                    // Realizar búsqueda con criterios relajados
                    var resultadosAlternativos = await _acompananteRepository.SearchAdvancedAsync(
                        searchText: criteriosRelajados.SearchText,
                        matchExactPhrase: criteriosRelajados.MatchExactPhrase ?? false,
                        ciudad: criteriosRelajados.Ciudad,
                        pais: criteriosRelajados.Pais,
                        latitud: criteriosRelajados.Latitud,
                        longitud: criteriosRelajados.Longitud,
                        radioKm: criteriosRelajados.RadioKm,
                        genero: criteriosRelajados.Genero,
                        edadMinima: criteriosRelajados.EdadMinima,
                        edadMaxima: criteriosRelajados.EdadMaxima,
                        alturaMinima: criteriosRelajados.AlturaMinima,
                        alturaMaxima: criteriosRelajados.AlturaMaxima,
                        tarifaMinima: criteriosRelajados.TarifaMinima,
                        tarifaMaxima: criteriosRelajados.TarifaMaxima,
                        moneda: criteriosRelajados.Moneda,
                        categoriaIds: criteriosRelajados.CategoriaIds,
                        servicioIds: criteriosRelajados.ServicioIds,
                        idiomasRequeridos: criteriosRelajados.IdiomasRequeridos,
                        soloVerificados: criteriosRelajados.SoloVerificados ?? false,
                        soloDisponibles: criteriosRelajados.SoloDisponibles ?? true,
                        conAgencia: criteriosRelajados.ConAgencia,
                        agenciaId: criteriosRelajados.AgenciaId,
                        soloConFotos: criteriosRelajados.SoloConFotos ?? false,
                        minimoFotos: criteriosRelajados.MinimoFotos,
                        pageNumber: criteriosRelajados.PageNumber,
                        pageSize: criteriosRelajados.PageSize,
                        orderBy: criteriosRelajados.OrderBy);

                    // Si encontramos alternativas, mapearlas y sugerirlas
                    if (resultadosAlternativos.Any())
                    {
                        _logger.LogInformation("Se encontraron {Count} resultados alternativos", resultadosAlternativos.Count);

                        // Mapear resultados alternativos
                        var alternativasDtos = new List<AcompananteSearchResultDto>();
                        foreach (var acompanante in resultadosAlternativos)
                        {
                            try
                            {
                                var dto = _mapper.Map<AcompananteSearchResultDto>(acompanante);

                                // Obtener foto principal
                                if (acompanante.fotos != null && acompanante.fotos.Any())
                                {
                                    var fotoPrincipal = acompanante.fotos.FirstOrDefault(f => f.es_principal == true);
                                    dto.FotoPrincipalUrl = fotoPrincipal?.url ?? acompanante.fotos.FirstOrDefault()?.url;
                                    dto.TotalFotos = acompanante.fotos.Count;
                                }

                                // Obtener nombres de categorías
                                if (acompanante.acompanante_categoria != null && acompanante.acompanante_categoria.Any())
                                {
                                    dto.Categorias = acompanante.acompanante_categoria
                                        .Where(c => c.categoria != null)
                                        .Select(c => c.categoria!.nombre)
                                        .ToList();
                                }

                                // Datos básicos de actividad
                                dto.ScoreActividad = acompanante.score_actividad ?? 0;

                                // Agregar mensaje sobre por qué se sugiere este perfil
                                dto.NotaRecomendacion = GenerarNotaRecomendacion(acompanante, criteria);

                                // Si hay coordenadas, calcular y mostrar la distancia
                                if (criteria.Latitud.HasValue && criteria.Longitud.HasValue &&
                                    acompanante.latitud.HasValue && acompanante.longitud.HasValue)
                                {
                                    double distanciaKm = GeoUtils.CalculateDistance(
                                        criteria.Latitud.Value, criteria.Longitud.Value,
                                        acompanante.latitud.Value, acompanante.longitud.Value);

                                    dto.NotaRecomendacion += $" (a {distanciaKm:F1} km de tu ubicación)";
                                }

                                alternativasDtos.Add(dto);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error al mapear acompañante alternativo {Id}", acompanante.id);
                                // Continuar con el siguiente
                            }
                        }

                        // Generar mensaje personalizado sobre qué filtros relajamos
                        string mensajeSugerencia = GenerarMensajeSugerenciaFiltros(criteria);

                        // Crear DTO con sugerencias
                        var searchSuggestions = new SearchSuggestionsResultDto
                        {
                            Mensaje = $"No se encontraron resultados para tu búsqueda. {mensajeSugerencia}",
                            ResultadosAlternativos = alternativasDtos,
                            FiltrosRelajados = true,
                            CriteriosOriginales = criteria,
                            CriteriosRelajados = criteriosRelajados
                        };

                        // Registrar la búsqueda original que no dio resultados
                        await LogSearchAsync(GetUserIdFromContext(), criteria, 0);

                        // Devolver respuesta con sugerencias
                        return new PaginatedResultDto<AcompananteSearchResultDto>
                        {
                            Items = new List<AcompananteSearchResultDto>(), // Lista vacía para resultados principales
                            TotalItems = 0,
                            PageNumber = criteria.PageNumber,
                            PageSize = criteria.PageSize,
                            SugerenciasBusqueda = searchSuggestions
                        };
                    }
                    else
                    {
                        _logger.LogInformation("No se encontraron resultados alternativos");

                        // No hay resultados ni siquiera con criterios relajados
                        var searchSuggestions = new SearchSuggestionsResultDto
                        {
                            Mensaje = "No se encontraron resultados para tu búsqueda. Intenta con términos más generales o elimina algunos filtros.",
                            ResultadosAlternativos = new List<AcompananteSearchResultDto>(),
                            FiltrosRelajados = true
                        };

                        // Registrar la búsqueda sin resultados
                        await LogSearchAsync(GetUserIdFromContext(), criteria, 0);

                        return new PaginatedResultDto<AcompananteSearchResultDto>
                        {
                            Items = new List<AcompananteSearchResultDto>(),
                            TotalItems = 0,
                            PageNumber = criteria.PageNumber,
                            PageSize = criteria.PageSize,
                            SugerenciasBusqueda = searchSuggestions
                        };
                    }
                }
                // *** FIN - NUEVA FUNCIONALIDAD PARA RESULTADOS ALTERNATIVOS ***

                // Mapear resultados normales
                var resultDtos = new List<AcompananteSearchResultDto>();
                foreach (var acompanante in acompanantes)
                {
                    try
                    {
                        _logger.LogDebug("Mapeando acompañante ID: {Id}, Nombre: {Nombre}",
                            acompanante.id, acompanante.nombre_perfil);

                        var dto = _mapper.Map<AcompananteSearchResultDto>(acompanante);
                        _logger.LogDebug("Mapeo básico completado para acompañante {Id}", acompanante.id);

                        // Obtener foto principal
                        if (acompanante.fotos != null && acompanante.fotos.Any())
                        {
                            var fotoPrincipal = acompanante.fotos.FirstOrDefault(f => f.es_principal == true);
                            dto.FotoPrincipalUrl = fotoPrincipal?.url ?? acompanante.fotos.FirstOrDefault()?.url;
                            dto.TotalFotos = acompanante.fotos.Count;
                            _logger.LogDebug("Foto principal asignada: {Url}", dto.FotoPrincipalUrl);
                        }
                        else
                        {
                            _logger.LogWarning("Acompañante {Id} no tiene fotos", acompanante.id);
                        }

                        // Obtener nombres de categorías
                        if (acompanante.acompanante_categoria != null && acompanante.acompanante_categoria.Any())
                        {
                            dto.Categorias = acompanante.acompanante_categoria
                                .Where(c => c.categoria != null)
                                .Select(c => c.categoria!.nombre)
                                .ToList();
                            _logger.LogDebug("Categorías asignadas: {CatCount}", dto.Categorias.Count);
                        }

                        // Obtener nombres de servicios
                        if (acompanante.servicios != null && acompanante.servicios.Any())
                        {
                            dto.Servicios = acompanante.servicios
                                .Select(s => s.nombre)
                                .ToList();
                            _logger.LogDebug("Servicios asignados: {ServCount}", dto.Servicios.Count);
                        }

                        // Datos de agencia si tiene
                        if (acompanante.agencia != null)
                        {
                            dto.NombreAgencia = acompanante.agencia.nombre;
                            dto.AgenciaVerificada = acompanante.agencia.esta_verificada ?? false;
                            _logger.LogDebug("Agencia asignada: {Nombre}", dto.NombreAgencia);
                        }

                        // Métricas de actividad - Usar valores de la entidad para evitar consultas adicionales
                        dto.ScoreActividad = acompanante.score_actividad ?? 0;

                        // Cargar visitas y contactos desde la entidad si están incluidas, o hacer consultas separadas
                        dto.TotalVisitas = acompanante.visitas_perfils?.Count ??
                            await _visitaRepository.GetTotalByAcompananteIdAsync(acompanante.id);

                        dto.TotalContactos = acompanante.contactos?.Count ??
                            await _contactoRepository.GetTotalByAcompananteIdAsync(acompanante.id);

                        _logger.LogDebug("Métricas asignadas: Visitas={Visitas}, Contactos={Contactos}",
                            dto.TotalVisitas, dto.TotalContactos);

                        // Si hay coordenadas de búsqueda y el acompañante tiene coordenadas, calcular y mostrar distancia
                        if (criteria.Latitud.HasValue && criteria.Longitud.HasValue &&
                            acompanante.latitud.HasValue && acompanante.longitud.HasValue)
                        {
                            double distanciaKm = GeoUtils.CalculateDistance(
                                criteria.Latitud.Value, criteria.Longitud.Value,
                                acompanante.latitud.Value, acompanante.longitud.Value);

                            dto.Distancia = distanciaKm;
                            _logger.LogDebug("Distancia calculada: {Distancia}km", distanciaKm);
                        }

                        resultDtos.Add(dto);
                        _logger.LogDebug("DTO añadido a resultados para acompañante {Id}", acompanante.id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al mapear acompañante {Id}", acompanante.id);
                        // Continuar con el siguiente acompañante
                    }
                }

                _logger.LogInformation("Mapeo completo. Total DTOs creados: {Count}", resultDtos.Count);

                // Registrar búsqueda exitosa
                try
                {
                    await LogSearchAsync(GetUserIdFromContext(), criteria, totalItems);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar la búsqueda");
                }

                return new PaginatedResultDto<AcompananteSearchResultDto>
                {
                    Items = resultDtos,
                    TotalItems = totalItems,
                    PageNumber = criteria.PageNumber,
                    PageSize = criteria.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar búsqueda avanzada");
                throw;
            }
        }
        // Método auxiliar para obtener el ID del usuario del contexto
        private int? GetUserIdFromContext()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var userIdClaim = httpContext?.User?.Claims
                    .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    return parsedUserId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Método para generar una nota de recomendación personalizada
        private string GenerarNotaRecomendacion(acompanante acompanante, AdvancedSearchCriteriaDto criteriaOriginal)
        {
            var razones = new List<string>();

            // Razones basadas en ubicación
            if (!string.IsNullOrEmpty(criteriaOriginal.Ciudad) && !string.IsNullOrEmpty(acompanante.ciudad))
            {
                if (criteriaOriginal.Ciudad.Equals(acompanante.ciudad, StringComparison.OrdinalIgnoreCase))
                {
                    razones.Add($"está en {acompanante.ciudad}");
                }
                else if (!string.IsNullOrEmpty(criteriaOriginal.Pais) && !string.IsNullOrEmpty(acompanante.pais) &&
                        criteriaOriginal.Pais.Equals(acompanante.pais, StringComparison.OrdinalIgnoreCase))
                {
                    razones.Add($"está en {acompanante.ciudad}, {acompanante.pais}");
                }
            }

            // Razones basadas en características
            if (!string.IsNullOrEmpty(criteriaOriginal.Genero) && !string.IsNullOrEmpty(acompanante.genero) &&
                criteriaOriginal.Genero.Equals(acompanante.genero, StringComparison.OrdinalIgnoreCase))
            {
                razones.Add("coincide con el género buscado");
            }

            // Razones basadas en verificación
            if (criteriaOriginal.SoloVerificados == true && acompanante.esta_verificado == true)
            {
                razones.Add("está verificado/a");
            }

            // Si tiene muchas fotos
            if (acompanante.fotos != null && acompanante.fotos.Count >= 5)
            {
                razones.Add($"tiene {acompanante.fotos.Count} fotos");
            }

            // Si tiene alta actividad
            if (acompanante.score_actividad.HasValue && acompanante.score_actividad > 50)
            {
                razones.Add("es muy popular");
            }

            // Si no hay razones específicas
            if (!razones.Any())
            {
                return "Perfil sugerido basado en tus criterios de búsqueda";
            }

            return $"Sugerido porque {string.Join(", ", razones)}";
        }
        // Método para generar un mensaje de sugerencia sobre qué filtros se podrían relajar
        private string GenerarMensajeSugerenciaFiltros(AdvancedSearchCriteriaDto criteria)
        {
            var sugerencias = new List<string>();

            // Sugerencias sobre ubicación
            if (!string.IsNullOrEmpty(criteria.Ciudad))
            {
                sugerencias.Add($"buscar más allá de {criteria.Ciudad}");
            }

            // Sugerencias sobre categorías
            if (criteria.CategoriaIds != null && criteria.CategoriaIds.Any())
            {
                sugerencias.Add("eliminar algunos filtros de categorías");
            }

            // Sugerencias sobre idiomas
            if (criteria.IdiomasRequeridos != null && criteria.IdiomasRequeridos.Any())
            {
                sugerencias.Add("reducir los requisitos de idiomas");
            }

            // Sugerencias sobre precio
            if (criteria.TarifaMinima.HasValue && criteria.TarifaMaxima.HasValue)
            {
                sugerencias.Add("ampliar el rango de precios");
            }

            // Si hay varias sugerencias
            if (sugerencias.Count > 1)
            {
                return $"Te sugerimos {string.Join(" o ", sugerencias)} para encontrar más resultados.";
            }
            // Si solo hay una sugerencia
            else if (sugerencias.Count == 1)
            {
                return $"Te sugerimos {sugerencias[0]} para encontrar más resultados.";
            }
            // Si no hay sugerencias específicas
            else
            {
                return "Intenta con términos más generales o menos filtros para encontrar resultados.";
            }
        }
        public async Task<int> SaveSearchAsync(int userId, AdvancedSearchCriteriaDto criteria, string name)
        {
            try
            {
                var busqueda = new busqueda_guardada
                {
                    usuario_id = userId,
                    nombre = name,
                    criterios_json = JsonSerializer.Serialize(criteria),
                    fecha_creacion = DateTime.UtcNow,
                    fecha_ultimo_uso = DateTime.UtcNow,
                    veces_usada = 1
                };

                return await _busquedaRepository.SaveSearchAsync(busqueda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar búsqueda para usuario {UserId}", userId);
                throw;
            }
        }


        public async Task<List<SavedSearchDto>> GetSavedSearchesAsync(int userId)
        {
            try
            {
                var busquedas = await _busquedaRepository.GetByUsuarioIdAsync(userId);

                var result = new List<SavedSearchDto>();
                foreach (var busqueda in busquedas)
                {
                    try
                    {
                        var criteria = JsonSerializer.Deserialize<AdvancedSearchCriteriaDto>(busqueda.criterios_json);
                        if (criteria != null)
                        {
                            result.Add(new SavedSearchDto
                            {
                                Id = busqueda.id,
                                Name = busqueda.nombre,
                                Criteria = criteria,
                                CreatedAt = busqueda.fecha_creacion,
                                ResultCount = busqueda.cantidad_resultados ?? 0,
                                LastUsed = busqueda.fecha_ultimo_uso ?? busqueda.fecha_creacion
                            });
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Error al deserializar criterios de búsqueda para id {BusquedaId}", busqueda.id);
                        // Continuar con la siguiente búsqueda
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener búsquedas guardadas para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteSavedSearchAsync(int userId, int searchId)
        {
            try
            {
                return await _busquedaRepository.DeleteAsync(searchId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar búsqueda guardada {SearchId} para usuario {UserId}", searchId, userId);
                throw;
            }
        }


        public async Task<List<SearchSuggestionDto>> GetSearchSuggestionsAsync(string partialCriteria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partialCriteria) || partialCriteria.Length < 2)
                {
                    return new List<SearchSuggestionDto>();
                }

                string cacheKey = $"{CACHE_KEY_SUGGESTIONS}_{partialCriteria.ToLower()}";

                // Intentar obtener sugerencias de la caché
                if (_cache.TryGetValue(cacheKey, out List<SearchSuggestionDto> cachedSuggestions))
                {
                    return cachedSuggestions;
                }

                var suggestions = new List<SearchSuggestionDto>();

                // Obtener sugerencias detalladas para ciudades usando el nuevo método
                var ciudadesSugerencias = await _busquedaRepository.GetDetailedAutocompleteSuggestionsAsync(
                    partialCriteria, "ciudad", 5);

                foreach (var ciudad in ciudadesSugerencias)
                {
                    suggestions.Add(new SearchSuggestionDto
                    {
                        Text = ciudad.Text,
                        Type = "Ciudad",
                        ResultCount = ciudad.Count,
                        UsageCount = 0
                    });
                }

                // Obtener sugerencias detalladas para categorías usando el nuevo método
                var categoriasSugerencias = await _busquedaRepository.GetDetailedAutocompleteSuggestionsAsync(
                    partialCriteria, "categoria", 5);

                foreach (var categoria in categoriasSugerencias)
                {
                    suggestions.Add(new SearchSuggestionDto
                    {
                        Text = categoria.Text,
                        Type = "Categoría",
                        ResultCount = categoria.Count,
                        UsageCount = 0,
                    });
                }

                // Obtener términos de búsqueda populares usando el método existente
                var terminos = await _busquedaRepository.GetAutocompleteSuggestionsAsync(
                    partialCriteria, "termino", 5);

                foreach (var termino in terminos)
                {
                    suggestions.Add(new SearchSuggestionDto
                    {
                        Text = termino,
                        Type = "Búsqueda popular",
                        ResultCount = 0,
                        UsageCount = 1
                    });
                }

                // Guardar en caché
                _cache.Set(cacheKey, suggestions, CACHE_DURATION);

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sugerencias de búsqueda para '{PartialCriteria}'", partialCriteria);
                return new List<SearchSuggestionDto>();
            }
        }
        public async Task LogSearchAsync(int? userId, AdvancedSearchCriteriaDto criteria, int results)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
                string userAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "unknown";

                var registro = new registro_busqueda
                {
                    usuario_id = userId,
                    fecha_busqueda = DateTime.UtcNow,
                    criterios_json = JsonSerializer.Serialize(criteria),
                    cantidad_resultados = results,
                    ip_busqueda = ipAddress,
                    user_agent = userAgent
                };

                await _busquedaRepository.LogSearchAsync(registro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar búsqueda para usuario {UserId}", userId);
                // No relanzar la excepción para evitar afectar al usuario
            }
        }



    }
}
