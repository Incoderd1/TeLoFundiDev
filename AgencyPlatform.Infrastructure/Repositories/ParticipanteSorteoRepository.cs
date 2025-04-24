using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class ParticipanteSorteoRepository : IParticipanteSorteoRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public ParticipanteSorteoRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<participantes_sorteo>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.participantes_sorteos
                .Include(p => p.sorteo)
                .Where(p => p.cliente_id == clienteId)
                .ToListAsync();
        }

        public async Task<participantes_sorteo> GetByClienteYSorteoAsync(int clienteId, int sorteoId)
        {
            return await _context.participantes_sorteos
                .FirstOrDefaultAsync(p => p.cliente_id == clienteId && p.sorteo_id == sorteoId);
        }

        public async Task<int> CountBySorteoIdAsync(int sorteoId)
        {
            return await _context.participantes_sorteos
                .CountAsync(p => p.sorteo_id == sorteoId);
        }

        public async Task AddAsync(participantes_sorteo participante)
        {
            await _context.participantes_sorteos.AddAsync(participante);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
