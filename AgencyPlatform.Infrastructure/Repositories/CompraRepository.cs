using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class CompraRepository : ICompraRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public CompraRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<compras_paquete> GetByIdAsync(int id)
        {
            return await _context.compras_paquetes
                .Include(c => c.paquete)
                .FirstOrDefaultAsync(c => c.id == id);
        }

        public async Task<List<compras_paquete>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.compras_paquetes
                .Include(c => c.paquete)
                .Where(c => c.cliente_id == clienteId)
                .OrderByDescending(c => c.fecha_compra)
                .ToListAsync();
        }

        public async Task AddAsync(compras_paquete compra)
        {
            await _context.compras_paquetes.AddAsync(compra);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<compras_paquete> GetByReferenciaAsync(string referencia)
        {
            return await _context.compras_paquetes
                .FirstOrDefaultAsync(c => c.referencia_pago == referencia);
        }
        public async Task UpdateAsync(compras_paquete compra)
        {
            _context.compras_paquetes.Update(compra);
        }
    }
}
