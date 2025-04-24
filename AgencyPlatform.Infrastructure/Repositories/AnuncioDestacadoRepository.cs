using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class AnuncioDestacadoRepository : IAnuncioDestacadoRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public AnuncioDestacadoRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        // Implementación para la interfaz IAnuncioDestacadoRepository
        public async Task<anuncios_destacado?> GetByIdAsync(int id)
        {
            return await _context.anuncios_destacados.FindAsync(id);
        }

        public async Task<List<anuncios_destacado>> GetAllAsync()
        {
            return await _context.anuncios_destacados.ToListAsync();
        }

        public async Task AddAsync(anuncios_destacado entity)
        {
            await _context.anuncios_destacados.AddAsync(entity);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(anuncios_destacado entity)
        {
            _context.anuncios_destacados.Update(entity);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(anuncios_destacado entity)
        {
            _context.anuncios_destacados.Remove(entity);
            await SaveChangesAsync();
        }

        public async Task<List<anuncios_destacado>> GetActivosAsync()
        {
            return await _context.anuncios_destacados
                .Where(a => (a.esta_activo ?? false) && a.fecha_fin > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<anuncios_destacado>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.anuncios_destacados
                .Where(a => a.acompanante_id == acompananteId)
                .ToListAsync();
        }

        public async Task<List<anuncios_destacado>> GetByAgenciaIdAsync(int agenciaId)
        {
            var acompananteIds = await _context.acompanantes
                .Where(a => a.agencia_id == agenciaId)
                .Select(a => a.id)
                .ToListAsync();

            return await _context.anuncios_destacados
                .Where(a => acompananteIds.Contains(a.acompanante_id))
                .ToListAsync();
        }

        public async Task<int> CountActivosByAgenciaIdAsync(int agenciaId)
        {
            var acompananteIds = await _context.acompanantes
                .Where(a => a.agencia_id == agenciaId)
                .Select(a => a.id)
                .ToListAsync();

            return await _context.anuncios_destacados
                .Where(a => acompananteIds.Contains(a.acompanante_id) &&
                      (a.esta_activo ?? false) &&
                      a.fecha_fin > DateTime.UtcNow)
                .CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Implementación para IGenericRepository<AnuncioDestacado>
        Task<AnuncioDestacado> IGenericRepository<AnuncioDestacado>.GetByIdAsync(int id)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task<IList<AnuncioDestacado>> IGenericRepository<AnuncioDestacado>.GetAllAsync()
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task<IList<AnuncioDestacado>> IGenericRepository<AnuncioDestacado>.FindAsync(Expression<Func<AnuncioDestacado, bool>> predicate)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task<AnuncioDestacado> IGenericRepository<AnuncioDestacado>.SingleOrDefaultAsync(Expression<Func<AnuncioDestacado, bool>> predicate)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task IGenericRepository<AnuncioDestacado>.AddAsync(AnuncioDestacado entity)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task IGenericRepository<AnuncioDestacado>.AddRangeAsync(IEnumerable<AnuncioDestacado> entities)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task IGenericRepository<AnuncioDestacado>.UpdateAsync(AnuncioDestacado entity)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task IGenericRepository<AnuncioDestacado>.DeleteAsync(AnuncioDestacado entity)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }

        Task IGenericRepository<AnuncioDestacado>.DeleteRangeAsync(IEnumerable<AnuncioDestacado> entities)
        {
            throw new NotImplementedException("Esta interfaz no se utiliza en esta implementación");
        }
    }
}