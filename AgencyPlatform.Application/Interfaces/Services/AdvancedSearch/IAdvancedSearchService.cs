using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Busqueda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.AdvancedSearch
{
    public interface IAdvancedSearchService
    {
        /// <summary>
        /// Realiza una búsqueda avanzada de acompañantes
        /// </summary>
        /// <param name="criteria">Criterios avanzados de búsqueda</param>
        /// <returns>Resultado paginado de acompañantes que cumplen los criterios</returns>
        Task<PaginatedResultDto<AcompananteSearchResultDto>> SearchAcompanantesAsync(AdvancedSearchCriteriaDto criteria);

        /// <summary>
        /// Guarda una búsqueda para un usuario registrado
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="criteria">Criterios de búsqueda</param>
        /// <param name="name">Nombre para guardar la búsqueda</param>
        /// <returns>ID de la búsqueda guardada</returns>
        Task<int> SaveSearchAsync(int userId, AdvancedSearchCriteriaDto criteria, string name);

        /// <summary>
        /// Obtiene las búsquedas guardadas por un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de búsquedas guardadas</returns>
        Task<List<SavedSearchDto>> GetSavedSearchesAsync(int userId);

        /// <summary>
        /// Elimina una búsqueda guardada
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="searchId">ID de la búsqueda</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteSavedSearchAsync(int userId, int searchId);

        /// <summary>
        /// Obtiene sugerencias de búsqueda basadas en criterios parciales
        /// </summary>
        /// <param name="partialCriteria">Criterio parcial (ej: texto de búsqueda, ciudad)</param>
        /// <returns>Lista de sugerencias de búsqueda</returns>
        Task<List<SearchSuggestionDto>> GetSearchSuggestionsAsync(string partialCriteria);

        /// <summary>
        /// Registra una búsqueda realizada para análisis y mejora de sugerencias
        /// </summary>
        /// <param name="userId">ID del usuario (opcional)</param>
        /// <param name="criteria">Criterios utilizados</param>
        /// <param name="results">Cantidad de resultados obtenidos</param>
        Task LogSearchAsync(int? userId, AdvancedSearchCriteriaDto criteria, int results);
    }
}
