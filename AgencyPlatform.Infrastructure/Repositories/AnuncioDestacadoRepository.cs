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

        // Métodos específicos de IAnuncioDestacadoRepository
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

        public async Task<int> DesactivarAnunciosVencidosAsync(DateTime fecha)
        {
            var anunciosVencidos = await _context.anuncios_destacados
                .Where(a => a.esta_activo == true && a.fecha_fin < fecha)
                .ToListAsync();

            foreach (var anuncio in anunciosVencidos)
            {
                anuncio.esta_activo = false;
                anuncio.updated_at = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return anunciosVencidos.Count;
        }

        public async Task<int> ActivarAnunciosProgramadosAsync(DateTime fecha)
        {
            var anunciosProgramados = await _context.anuncios_destacados
                .Where(a => a.esta_activo == false &&
                           a.fecha_inicio <= fecha &&
                           a.fecha_fin > fecha)
                .ToListAsync();

            foreach (var anuncio in anunciosProgramados)
            {
                anuncio.esta_activo = true;
                anuncio.updated_at = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return anunciosProgramados.Count;
        }

        public async Task<bool> TieneAnuncioActivoAsync(int acompananteId)
        {
            return await _context.anuncios_destacados
                .AnyAsync(a => a.acompanante_id == acompananteId &&
                            a.esta_activo == true &&
                            a.fecha_fin > DateTime.UtcNow);
        }

        public async Task<List<anuncios_destacado>> GetAnunciosDestacadosByAgenciaIdAsync(int agenciaId)
        {
            return await _context.anuncios_destacados
                .Include(a => a.acompanante)
                .Where(a => a.acompanante.agencia_id == agenciaId)
                .ToListAsync();
        }

        public async Task<List<anuncios_destacado>> GetActivosPorTipoYFechasAsync(string tipo, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.anuncios_destacados
                .Where(a => a.tipo == tipo
                         && a.esta_activo == true
                         && a.fecha_inicio < fechaFin
                         && a.fecha_fin > fechaInicio)
                .ToListAsync();
        }

        public async Task<anuncios_destacado> GetByReferenceIdAsync(string referenceId)
        {
            return await _context.anuncios_destacados
                .FirstOrDefaultAsync(a => a.payment_reference == referenceId);
        }

        // Implementación de los métodos de IGenericRepository<anuncios_destacado>
        async Task<anuncios_destacado> IGenericRepository<anuncios_destacado>.GetByIdAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        async Task<IList<anuncios_destacado>> IGenericRepository<anuncios_destacado>.GetAllAsync()
        {
            return await GetAllAsync();
        }

        async Task<IList<anuncios_destacado>> IGenericRepository<anuncios_destacado>.FindAsync(Expression<Func<anuncios_destacado, bool>> predicate)
        {
            return await _context.anuncios_destacados
                .Where(predicate)
                .ToListAsync();
        }

        async Task<anuncios_destacado> IGenericRepository<anuncios_destacado>.SingleOrDefaultAsync(Expression<Func<anuncios_destacado, bool>> predicate)
        {
            return await _context.anuncios_destacados
                .SingleOrDefaultAsync(predicate);
        }

        async Task IGenericRepository<anuncios_destacado>.AddAsync(anuncios_destacado entity)
        {
            await AddAsync(entity);
        }

        async Task IGenericRepository<anuncios_destacado>.AddRangeAsync(IEnumerable<anuncios_destacado> entities)
        {
            await _context.anuncios_destacados.AddRangeAsync(entities);
            await SaveChangesAsync();
        }

        async Task IGenericRepository<anuncios_destacado>.UpdateAsync(anuncios_destacado entity)
        {
            await UpdateAsync(entity);
        }

        async Task IGenericRepository<anuncios_destacado>.DeleteAsync(anuncios_destacado entity)
        {
            await DeleteAsync(entity);
        }

        async Task IGenericRepository<anuncios_destacado>.DeleteRangeAsync(IEnumerable<anuncios_destacado> entities)
        {
            _context.anuncios_destacados.RemoveRange(entities);
            await SaveChangesAsync();
        }
    }
}