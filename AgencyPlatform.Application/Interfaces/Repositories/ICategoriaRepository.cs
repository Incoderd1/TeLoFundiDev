using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ICategoriaRepository
    {
        Task<List<categoria>> GetAllAsync();
        Task<List<categoria>> GetPagedAsync(int skip, int take);
        Task<categoria> GetByIdAsync(int id);
        Task<categoria> GetByNombreAsync(string nombre);
        Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null);
        Task<int> GetTotalAcompanantesAsync(int categoriaId);
        Task<int> GetTotalVisitasAsync(int categoriaId);
        Task<List<categoria>> GetMasPopularesAsync(int cantidad = 5);
        Task<List<categoria>> BuscarPorNombreAsync(string termino);
        Task AddAsync(categoria entity);
        Task UpdateAsync(categoria entity);
        Task DeleteAsync(categoria entity);
        Task SaveChangesAsync();
    }
}
