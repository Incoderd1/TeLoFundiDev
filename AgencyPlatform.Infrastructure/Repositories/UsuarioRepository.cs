using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
namespace AgencyPlatform.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public UsuarioRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<usuario?> ObtenerPorEmailAsync(string email)
        {
            return await _context.usuarios.FirstOrDefaultAsync(u => u.email == email);
        }

        public async Task<usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.usuarios.FindAsync(id);
        }

        public async Task<IEnumerable<usuario>> ObtenerTodosAsync(int page, int pageSize)
        {
            return await _context.usuarios
                .OrderBy(u => u.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task CrearAsync(usuario usuario)
        {
            await _context.usuarios.AddAsync(usuario);
        }

        public async Task EliminarAsync(usuario usuario)
        {
            _context.usuarios.Remove(usuario);
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<usuario> GetByIdAsync(int id)
        {
            return await _context.usuarios.FindAsync(id);
        }
        

        public Task<string> GetRolNameByUserIdAsync(int usuarioId)
        {
            throw new NotImplementedException();
        }
        public async Task<List<string>> GetRolesAsync(int usuarioId)
        {
            var usuario = await _context.usuarios
                .Include(u => u.rol)
                .FirstOrDefaultAsync(u => u.id == usuarioId);

            if (usuario == null || usuario.rol == null)
                return new List<string>();

            return new List<string> { usuario.rol.nombre };
        }
    }
}

