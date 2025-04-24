using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces
{
    public interface ICompraRepository
    {
        Task<compras_paquete> GetByIdAsync(int id);
        Task<List<compras_paquete>> GetByClienteIdAsync(int clienteId);
        Task AddAsync(compras_paquete compra);
        Task<bool> SaveChangesAsync();

        Task<compras_paquete> GetByReferenciaAsync(string referencia);
        Task UpdateAsync(compras_paquete compra);

    }
}
