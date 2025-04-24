using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class FotoRepository : IFotoRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public FotoRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<foto>> GetAllAsync()
        {
            return await _context.fotos.ToListAsync();
        }

        public async Task<foto?> GetByIdAsync(int id)
        {
            return await _context.fotos.FirstOrDefaultAsync(f => f.id == id);
        }

        public async Task<List<foto>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.fotos
                .Where(f => f.acompanante_id == acompananteId)
                .OrderBy(f => f.orden)
                .ToListAsync();
        }

        public async Task<foto?> GetFotoPrincipalByAcompananteIdAsync(int acompananteId)
        {
            return await _context.fotos
                .FirstOrDefaultAsync(f => f.acompanante_id == acompananteId && f.es_principal == true);
        }

        public async Task<int> ContarFotosPorAcompananteAsync(int acompananteId)
        {
            return await _context.fotos
                .CountAsync(f => f.acompanante_id == acompananteId);
        }

        public async Task<foto?> GetPrimeraFotoNoEsPrincipalAsync(int acompananteId)
        {
            return await _context.fotos
                .FirstOrDefaultAsync(f => f.acompanante_id == acompananteId && f.es_principal != true);
        }

        public async Task QuitarFotosPrincipalesAsync(int acompananteId)
        {
            var fotos = await _context.fotos
                .Where(f => f.acompanante_id == acompananteId && f.es_principal == true)
                .ToListAsync();

            foreach (var foto in fotos)
            {
                foto.es_principal = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(foto entity)
        {
            await _context.fotos.AddAsync(entity);
        }

        public async Task UpdateAsync(foto entity)
        {
            _context.fotos.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(foto entity)
        {
            _context.fotos.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}