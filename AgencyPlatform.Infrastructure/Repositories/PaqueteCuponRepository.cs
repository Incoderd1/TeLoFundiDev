using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class PaqueteCuponRepository : IPaqueteCuponRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public PaqueteCuponRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<paquetes_cupone> GetByIdAsync(int id)
        {
            return await _context.paquetes_cupones
                .FirstOrDefaultAsync(p => p.id == id);
        }

        public async Task<List<paquetes_cupone>> GetActivosAsync()
        {
            return await _context.paquetes_cupones
                .Where(p => p.activo == true)
                .ToListAsync();
        }

        public async Task<List<paquete_cupones_detalle>> GetDetallesByPaqueteIdAsync(int paqueteId)
        {
            return await _context.paquete_cupones_detalles
                .Include(d => d.tipo_cupon)
                .Where(d => d.paquete_id == paqueteId)
                .ToListAsync();
        }
    }
}
