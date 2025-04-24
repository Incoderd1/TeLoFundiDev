using AgencyPlatform.Core.Entities;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IVisitaRepository
    {
        Task<List<visitas_perfil>> GetAllAsync();
        Task<visitas_perfil?> GetByIdAsync(int id);
        Task<List<visitas_perfil>> GetByAcompananteIdAsync(int acompananteId);
        Task<visitas_perfil?> GetVisitaRecienteAsync(int acompananteId, string? ipVisitante);
        Task<long> ContarVisitasTotalesAsync(int acompananteId);
        Task<long> ContarVisitasRecientesAsync(int acompananteId, int dias);
        Task AddAsync(visitas_perfil entity);
        Task UpdateAsync(visitas_perfil entity);
        Task DeleteAsync(visitas_perfil entity);
        Task SaveChangesAsync();

        Task<List<visitas_perfil>> GetByClienteIdAsync(int clienteId, int cantidad = 10);
        Task<List<visitas_perfil>> GetByAcompananteIdAsync(int acompananteId, int cantidad = 10);
        Task<int> CountByAcompananteIdAsync(int acompananteId);
        Task<bool> SaveChangesAsync2();

        Task<List<int>> GetPerfilesVisitadosRecientementeIdsByClienteAsync(int clienteId, int cantidad);

    }
}
