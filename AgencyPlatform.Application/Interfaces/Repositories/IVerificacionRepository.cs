using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IVerificacionRepository
    {
        Task<List<verificacione>> GetAllAsync();
        Task<verificacione?> GetByIdAsync(int id);
        Task<verificacione?> GetByAcompananteIdAsync(int acompananteId);
        Task<List<verificacione>> GetByAgenciaIdAsync(int agenciaId);
        Task<List<verificacione>> GetByAgenciaIdAndPeriodoAsync(int agenciaId, DateTime fechaInicio, DateTime fechaFin);
        Task AddAsync(verificacione entity);
        Task UpdateAsync(verificacione entity);
        Task DeleteAsync(verificacione entity);
        Task DeleteByAgenciaIdAsync(int agenciaId);
        Task SaveChangesAsync();

        // El método que ya estaba pero no definido correctamente
        // Task<verificacione?> GetVerificacionByAcompananteIdAsync(int acompananteId); // Está duplicado con GetByAcompananteIdAsync

        // Métodos adicionales que podrían ser necesarios para el servicio
        Task<List<verificacione>> GetVerificacionesByAgenciaIdAsync(int agenciaId);
        Task<List<verificacione>> GetVerificacionesByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);
        Task DeleteVerificacionesByAgenciaIdAsync(int agenciaId);

        Task<List<verificacione>> GetHistorialVerificacionesAsync(int acompananteId);

    }
}
