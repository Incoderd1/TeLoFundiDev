using AgencyPlatform.Core.Entities;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IParticipanteSorteoRepository
    {
        Task<List<participantes_sorteo>> GetByClienteIdAsync(int clienteId);
        Task<participantes_sorteo> GetByClienteYSorteoAsync(int clienteId, int sorteoId);
        Task<int> CountBySorteoIdAsync(int sorteoId);
        Task AddAsync(participantes_sorteo participante);
        Task<bool> SaveChangesAsync();
    }
}
