using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class SorteoRepository : ISorteoRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public SorteoRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<sorteo> GetByIdAsync(int id)
        {
            return await _context.sorteos
                .FirstOrDefaultAsync(s => s.id == id);
        }

        public async Task<List<sorteo>> GetActivosAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.sorteos
                .Where(s => s.esta_activo == true && s.fecha_inicio <= now && s.fecha_fin >= now)
                .ToListAsync();
        }

        public async Task<List<sorteo>> GetAllAsync()
        {
            return await _context.sorteos.ToListAsync();
        }
    }
}
