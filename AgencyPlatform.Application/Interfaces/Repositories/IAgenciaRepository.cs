using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IAgenciaRepository
    {
        // Métodos existentes
        Task<List<agencia>> GetAllAsync();
        Task<agencia?> GetByIdAsync(int id);
        Task<agencia?> GetByUsuarioIdAsync(int usuarioId);
        Task AddAsync(agencia entity);
        Task UpdateAsync(agencia entity);
        Task DeleteAsync(agencia entity);
        Task SaveChangesAsync();

        // Métodos adicionales para acceder a las vistas y nuevas funcionalidades
        Task<List<vw_agencias_acompanante>> GetAgenciasAcompanantesViewAsync();
        Task<vw_agencias_acompanante?> GetAgenciaAcompanantesViewByIdAsync(int agenciaId);

        // Métodos para acompañantes de agencia
        Task<List<acompanante>> GetAcompanantesByAgenciaIdAsync(int agenciaId);
        Task<List<acompanante>> GetAcompanantesVerificadosByAgenciaIdAsync(int agenciaId);

        // Métodos para verificaciones
        Task<verificacione?> GetVerificacionByAcompananteIdAsync(int acompananteId);
        Task AddVerificacionAsync(verificacione entity);

        // Métodos para anuncios destacados
        Task<List<anuncios_destacado>> GetAnunciosDestacadosByAgenciaIdAsync(int agenciaId);
        Task AddAnuncioDestacadoAsync(anuncios_destacado entity);

        // Métodos para comisiones
        Task<decimal> GetComisionPorcentajeByAgenciaIdAsync(int agenciaId);
        Task UpdateComisionPorcentajeAsync(int agenciaId, decimal porcentaje);

        // Método para estadísticas (para vista vw_agencias_acompanantes)
        Task<List<agencia>> GetAgenciasPendientesVerificacionAsync();

        Task<agencia?> GetAgenciaByUsuarioIdAsync(int usuarioId);


        Task<bool> ExisteSolicitudPendienteAsync(int acompananteId, int agenciaId);
        Task CrearSolicitudAsync(SolicitudAgencia solicitud);
        Task<List<SolicitudAgencia>> GetSolicitudesPendientesPorAgenciaAsync(int agenciaId);
        Task<SolicitudAgencia?> GetSolicitudByIdAsync(int solicitudId);

        Task UpdateSolicitudAsync(SolicitudAgencia solicitud);

        Task AddMovimientoPuntosAsync(movimientos_puntos_agencia movimiento);
        Task<List<movimientos_puntos_agencia>> GetUltimosMovimientosPuntosAsync(int agenciaId, int cantidad = 10);



    }
}
