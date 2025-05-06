using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ICuponClienteRepository
    {
        Task<List<cupones_cliente>> GetDisponiblesByClienteIdAsync(int clienteId);
        Task<cupones_cliente> GetByCodigoAsync(string codigo);
        Task<int> CountDisponiblesByClienteIdAsync(int clienteId);
        Task AddAsync(cupones_cliente cupon);
        Task UpdateAsync(cupones_cliente cupon);
        Task<bool> SaveChangesAsync();

        //nuevos
        Task<List<cupones_cliente>> GetExpiradosAsync(DateTime now);
        Task<int> EliminarCuponesVencidosNoUsadosAsync(DateTime fecha);

        Task<cupones_cliente> GetByIdAsync(int id); // Añadir este método


    }
}
