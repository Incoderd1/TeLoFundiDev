using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class CuponClienteRepository : ICuponClienteRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public CuponClienteRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<cupones_cliente>> GetDisponiblesByClienteIdAsync(int clienteId)
        {
            return await _context.cupones_clientes
                .Include(c => c.tipo_cupon)
                .Where(c => c.cliente_id == clienteId &&
                           (c.esta_usado != true) &&
                           c.fecha_expiracion > DateTime.UtcNow)
                .OrderBy(c => c.fecha_expiracion)
                .ToListAsync();
        }

        public async Task<cupones_cliente> GetByCodigoAsync(string codigo)
        {
            return await _context.cupones_clientes
                .Include(c => c.tipo_cupon)
                .FirstOrDefaultAsync(c => c.codigo == codigo);
        }

        public async Task<int> CountDisponiblesByClienteIdAsync(int clienteId)
        {
            return await _context.cupones_clientes
                .CountAsync(c => c.cliente_id == clienteId &&
                                (c.esta_usado != true) &&
                                c.fecha_expiracion > DateTime.UtcNow);
        }

        public async Task AddAsync(cupones_cliente cupon)
        {
            await _context.cupones_clientes.AddAsync(cupon);
        }

        public async Task UpdateAsync(cupones_cliente cupon)
        {
            _context.cupones_clientes.Update(cupon);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<cupones_cliente>> GetExpiradosAsync(DateTime now)
        {
            return await _context.cupones_clientes
                             .Where(c => c.esta_usado == false
                                         && c.fecha_expiracion < now)
                             .ToListAsync();
        }
        public async Task<int> EliminarCuponesVencidosNoUsadosAsync(DateTime fecha)
        {
            var cuponesVencidos = await _context.cupones_clientes
                .Where(c => c.esta_usado == false &&
                           c.fecha_expiracion < fecha)
                .ToListAsync();

            _context.cupones_clientes.RemoveRange(cuponesVencidos);
            await _context.SaveChangesAsync();

            return cuponesVencidos.Count;
        }
        public async Task<cupones_cliente> GetByIdAsync(int id)
        {
            return await _context.cupones_clientes.FirstOrDefaultAsync(c => c.id == id);
        }
    }
}
