using AgencyPlatform.Core.Entities;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IContactoRepository
    {
        Task<List<contacto>> GetByAcompananteIdAsync(int acompananteId);
        Task<contacto> GetByIdAsync(int id);
        Task AddAsync(contacto entity);
        Task UpdateAsync(contacto entity);
        Task DeleteAsync(contacto entity);
        Task SaveChangesAsync();
        Task<int> GetTotalByAcompananteIdAsync(int acompananteId);
        Task<int> GetTotalDesdeAsync(int acompananteId, DateTime fechaInicio);
        Task<Dictionary<string, int>> GetContactosPorTipoAsync(int acompananteId);

        Task<List<contacto>> GetByClienteIdAsync(int clienteId, int cantidad = 10);
        Task<int> CountByAcompananteIdAsync(int acompananteId);
        Task<bool> SaveChangesAsync2();
    }
}
