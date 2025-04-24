using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public CategoriaRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<categoria>> GetAllAsync()
        {
            return await _context.categorias
                .OrderBy(c => c.nombre)
                .ToListAsync();
        }

        public async Task<List<categoria>> GetPagedAsync(int skip, int take)
        {
            return await _context.categorias
                .OrderBy(c => c.nombre)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<categoria> GetByIdAsync(int id)
        {
            return await _context.categorias.FirstOrDefaultAsync(c => c.id == id);
        }

        public async Task<categoria> GetByNombreAsync(string nombre)
        {
            return await _context.categorias
                .FirstOrDefaultAsync(c => c.nombre.ToLower() == nombre.ToLower());
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null)
        {
            var query = _context.categorias.AsQueryable();

            if (exceptoId.HasValue)
                query = query.Where(c => c.id != exceptoId.Value);

            return await query.AnyAsync(c => c.nombre.ToLower() == nombre.ToLower());
        }

        public async Task<int> GetTotalAcompanantesAsync(int categoriaId)
        {
            return await _context.acompanante_categorias
                .Where(ac => ac.categoria_id == categoriaId)
                .CountAsync();
        }

        public async Task<int> GetTotalVisitasAsync(int categoriaId)
        {
            return await _context.acompanante_categorias
                .Where(ac => ac.categoria_id == categoriaId)
                .Join(_context.acompanantes,
                    ac => ac.acompanante_id,
                    a => a.id,
                    (ac, a) => a)
                .SelectMany(a => a.visitas_perfils)
                .CountAsync();
        }

        public async Task<List<categoria>> GetMasPopularesAsync(int cantidad = 5)
        {
            return await _context.categorias
                .OrderByDescending(c => _context.acompanante_categorias.Count(ac => ac.categoria_id == c.id))
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<categoria>> BuscarPorNombreAsync(string termino)
        {
            return await _context.categorias
                .Where(c => c.nombre.Contains(termino) || c.descripcion.Contains(termino))
                .OrderBy(c => c.nombre)
                .ToListAsync();
        }

        public async Task AddAsync(categoria entity)
        {
            await _context.categorias.AddAsync(entity);
        }

        public async Task UpdateAsync(categoria entity)
        {
            _context.categorias.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(categoria entity)
        {
            _context.categorias.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
