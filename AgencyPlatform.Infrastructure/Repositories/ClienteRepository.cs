using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public ClienteRepository(AgencyPlatformDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<cliente> GetByIdAsync(int id)
        {
            return await _context.clientes
                .AsNoTracking()
                .Include(c => c.usuario)
                .FirstOrDefaultAsync(c => c.id == id);
        }

        public async Task<cliente> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.clientes
                .AsNoTracking()
                .Include(c => c.usuario)
                .FirstOrDefaultAsync(c => c.usuario_id == usuarioId);
        }

        public async Task<bool> ExisteNicknameAsync(string nickname)
        {
            return await _context.clientes
                .AsNoTracking()
                .AnyAsync(c => c.nickname == nickname);
        }

        public Task AddAsync(cliente entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return _context.clientes.AddAsync(entity).AsTask();
        }

        public Task UpdateAsync(cliente entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.clientes.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            // Eliminación sin cargar la entidad completa
            var stub = new cliente { id = id };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync()
        {
            var cambios = await _context.SaveChangesAsync();
            if (cambios <= 0)
                throw new InvalidOperationException("No se guardaron los cambios en ClienteRepository");
            return true;
        }
    }
}
