using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Busqueda;
using AgencyPlatform.Application;
using AgencyPlatform.Application.Interfaces.Services.AdvancedSearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AgencyPlatform.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using AgencyPlatform.Core.Entities;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IAdvancedSearchService _searchService;
        private readonly ILogger<SearchController> _logger;
        private readonly AgencyPlatformDbContext _context;

        public SearchController(
            IAdvancedSearchService searchService,
            ILogger<SearchController> logger, AgencyPlatformDbContext context)
        {
            _searchService = searchService;
            _logger = logger;
            _context = context;
        }
      
        [HttpPost("advanced")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResultDto<AcompananteSearchResultDto>>> SearchAdvanced(
           [FromBody] AdvancedSearchCriteriaDto criteria)
        {
            try
            {
                var results = await _searchService.SearchAcompanantesAsync(criteria);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar búsqueda avanzada");
                return BadRequest(new { error = "Error al procesar la búsqueda. Inténtelo de nuevo." });
            }
        }
        [HttpGet("suggestions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SearchSuggestionDto>>> GetSuggestions(
           [FromQuery] string term)
        {
            var suggestions = await _searchService.GetSearchSuggestionsAsync(term);
            return Ok(suggestions);
        }
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> SaveSearch(
           [FromBody] SaveSearchDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Usuario no identificado correctamente" });
                }

                var searchId = await _searchService.SaveSearchAsync(userId, dto.Criteria, dto.Name);
                return Ok(searchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar búsqueda");
                return BadRequest(new { error = "Error al guardar la búsqueda. Inténtelo de nuevo." });
            }
        }
            [HttpGet("saved")]
            [Authorize]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<SavedSearchDto>>> GetSavedSearches()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Usuario no identificado correctamente" });
                }

                var searches = await _searchService.GetSavedSearchesAsync(userId);
                return Ok(searches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener búsquedas guardadas");
                return BadRequest(new { error = "Error al obtener las búsquedas guardadas. Inténtelo de nuevo." });
            }
        }
        [HttpDelete("saved/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteSavedSearch(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Usuario no identificado correctamente" });
                }

                var result = await _searchService.DeleteSavedSearchAsync(userId, id);

                if (!result)
                    return NotFound(new { error = "Búsqueda no encontrada o no pertenece al usuario" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar búsqueda guardada");
                return BadRequest(new { error = "Error al eliminar la búsqueda. Inténtelo de nuevo." });
            }
        }


        [HttpGet("test-geo-distance")]
        public ActionResult TestGeoDistance([FromQuery] double lat = 18.4861, [FromQuery] double lon = -69.9312, [FromQuery] int radioKm = 10)
        {
            // Cálculo de distancia de Haversine
            double CalcularDistanciaHaversine(double lat1, double lon1, double lat2, double lon2)
            {
                const double R = 6371; // Radio de la Tierra en km
                var dLat = (lat2 - lat1) * Math.PI / 180;
                var dLon = (lon2 - lon1) * Math.PI / 180;
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                return R * c;
            }

            try
            {
                // Obtener todos los perfiles con coordenadas
                var perfiles = _context.acompanantes
                    .Where(a => a.latitud != null && a.longitud != null)
                    .AsEnumerable() // Traer a memoria para cálculo
                    .Select(a => new {
                        a.id,
                        a.nombre_perfil,
                        a.latitud,
                        a.longitud,
                        distancia = CalcularDistanciaHaversine(lat, lon, a.latitud.Value, a.longitud.Value),
                        dentroDelRadio = CalcularDistanciaHaversine(lat, lon, a.latitud.Value, a.longitud.Value) <= radioKm
                    })
                    .ToList();

                // Filtrar los que están dentro del radio
                var perfilesDentroRadio = perfiles.Where(p => p.dentroDelRadio).ToList();

                return Ok(new
                {
                    coordenadasBusqueda = new { lat, lon, radioKm },
                    perfilesConDistancia = perfiles,
                    perfilesDentroDelRadio = perfilesDentroRadio,
                    totalPerfiles = perfiles.Count,
                    totalDentroRadio = perfilesDentroRadio.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stack = ex.StackTrace });
            }
        }
    }






}

