using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IAcompananteRepository
    {
        Task<List<acompanante>> GetAllAsync();
        Task<acompanante?> GetByIdAsync(int id);
        Task<acompanante?> GetByUsuarioIdAsync(int usuarioId);
        Task AddAsync(acompanante entity);
        Task UpdateAsync(acompanante entity);
        Task DeleteAsync(acompanante entity);
        Task SaveChangesAsync();
        Task AgregarCategoriaSinGuardarAsync(int acompananteId, int categoriaId);
        Task<List<acompanante>> GetDestacadosAsync();
        Task<List<acompanante>> GetRecientesAsync(int cantidad);
        Task<List<acompanante>> GetPopularesAsync(int cantidad);

        // Métodos para búsqueda y filtrado
        Task<List<acompanante>> BuscarAsync(string? busqueda, string? ciudad, string? pais, string? genero,
            int? edadMinima, int? edadMaxima, decimal? tarifaMinima, decimal? tarifaMaxima,
            bool? soloVerificados, bool? soloDisponibles, List<int>? categoriaIds,
            string? ordenarPor, int pagina, int elementosPorPagina);

        // Métodos para categorías
        Task<List<acompanante_categoria>> GetCategoriasByAcompananteIdAsync(int acompananteId);
        Task<bool> TieneCategoriaAsync(int acompananteId, int categoriaId);
        Task AgregarCategoriaAsync(int acompananteId, int categoriaId);
        Task EliminarCategoriaAsync(int acompananteId, int categoriaId);
        Task ActualizarScoreActividadAsync(int acompananteId, long scoreActividad);
        Task<bool> TieneAcompanantesAsync(int categoriaId);
        Task<PerfilEstadisticasDto?> GetEstadisticasPerfilAsync(int acompananteId);



        Task<int> CountByAgenciaIdAsync(int agenciaId);
        Task<int> CountVerificadosByAgenciaIdAsync(int agenciaId);
        Task<List<acompanante>> GetDestacadosByAgenciaIdAsync(int agenciaId, int limit = 5);

        Task<PaginatedResult<acompanante>> GetIndependientesAsync(
                int pageNumber,
                int pageSize,
                string filterBy,
                string sortBy,
                bool sortDesc);


        Task<List<acompanante>> GetMasVisitadosAsync(int cantidad);
        Task<List<acompanante>> GetByCategoriasAsync(List<int> categoriaIds, int cantidad);
        Task<List<acompanante>> GetByCiudadesAsync(List<string> ciudades, int cantidad);
        Task<List<int>> GetCategoriasByAcompananteIdAsync2(int acompananteId);
        Task<bool> TieneCategoriasAsync(int acompananteId, List<int> categoriaIds);


        Task<List<int>> GetPerfilesVisitadosRecientementeIdsByClienteAsync(int clienteId, int cantidad);
        Task<List<acompanante>> GetByIdsAsync(List<int> ids);
        Task<Dictionary<int, AcompananteEstadisticas>> GetEstadisticasMultiplesAsync(List<int> ids);


        // Agregar a IAcompananteRepository
        Task<List<int>> GetPerfilesVisitadosIdsByClienteAsync(int clienteId);
        Task<List<int>> GetCategoriasDePerfilesAsync(List<int> perfilesIds);
        Task<List<string>> GetCiudadesDePerfilesAsync(List<int> perfilesIds);
        Task<List<int>> GetIdsByCategoriasAsync(List<int> categoriaIds, int cantidad, List<int> excluirIds);
        Task<List<int>> GetIdsByCiudadesAsync(List<string> ciudades, int cantidad, List<int> excluirIds);
        Task<List<int>> GetIdsPopularesAsync(int cantidad, List<int> excluirIds = null);
        Task<List<int>> GetCategoriasIdsDePerfilAsync(int perfilId);

        Task<int> CountAsync(Func<acompanante, bool> predicate);
        Task<int> CountDestacadosAsync();
        Task<List<acompanante>> GetAllPaginatedAsync(int skip, int take);
        Task<List<acompanante>> GetAllPaginatedAsync(int skip, int take, Expression<Func<acompanante, bool>> predicate);

        Task<List<acompanante>> GetRecientesPaginadosAsync(int skip, int take);
        Task<List<acompanante>> GetPopularesPaginadosAsync(int skip, int take);
        Task<List<acompanante>> GetDestacadosPaginadosAsync(int skip, int take);

        Task<List<acompanante>> SearchAdvancedAsync(
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
            int pageSize = 20);

        Task<int> CountSearchAdvancedAsync(
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
            int? minimoFotos = null);

        Task<List<dynamic>> GetCiudadesConCoincidenciaAsync(string searchTerm, int limit);


        Task<List<acompanante>> GetAllActivosAsync();
        Task<List<acompanante>> GetPerfilesInactivosDesdeAsync(DateTime fechaLimite);
        Task<int> CountVisitasPeriodoAsync(int acompananteId, DateTime fechaInicio, DateTime fechaFin);
        Task<int> CountContactosPeriodoAsync(int acompananteId, DateTime fechaInicio, DateTime fechaFin);
        Task ActualizarVistaRankingPerfilesAsync();


        //Task<acompanante?> GetByStripeAccountIdAsync(string stripeAccountId);
        //Task<List<acompanante>> GetPendientesStripeOnboardingAsync();


        Task<acompanante> GetByStripeAccountIdAsync(string stripeAccountId);


    }
}
