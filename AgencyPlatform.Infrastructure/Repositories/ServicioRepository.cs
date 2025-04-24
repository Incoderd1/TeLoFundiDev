using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public ServicioRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<servicio>> GetAllAsync()
        {
            return await _context.servicios.ToListAsync();
        }

        public async Task<servicio?> GetByIdAsync(int id)
        {
            return await _context.servicios.FirstOrDefaultAsync(s => s.id == id);
        }

        public async Task<List<servicio>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.servicios
                .Where(s => s.acompanante_id == acompananteId)
                .OrderBy(s => s.nombre)
                .ToListAsync();
        }

        public async Task AddAsync(servicio entity)
        {
            await _context.servicios.AddAsync(entity);
        }

        public async Task UpdateAsync(servicio entity)
        {
            _context.servicios.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(servicio entity)
        {
            _context.servicios.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}