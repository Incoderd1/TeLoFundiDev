using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IMovimientoPuntosRepository
    {
        Task<List<movimientos_punto>> GetByClienteIdAsync(int clienteId, int cantidad = 10);
        Task<int> CountByClienteIdAndConceptoHoyAsync(int clienteId, string concepto);
        Task AddAsync(movimientos_punto movimiento);
        Task<bool> SaveChangesAsync();
    }
}
