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
    public class MovimientoPuntosRepository : IMovimientoPuntosRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public MovimientoPuntosRepository(AgencyPlatformDbContext context)
        {
            _context = context;    
        }

        public async Task<List<movimientos_punto>> GetByClienteIdAsync(int clienteId, int cantidad = 10)
        {
            return await _context.movimientos_puntos
                .Where(m => m.cliente_id == clienteId)
                .OrderByDescending(m => m.fecha)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<int> CountByClienteIdAndConceptoHoyAsync(int clienteId, string concepto)
        {
            var hoy = DateTime.UtcNow.Date;
            return await _context.movimientos_puntos
                .CountAsync(m => m.cliente_id == clienteId &&
                              m.concepto == concepto &&
                              m.fecha >= hoy);
        }

        public async Task AddAsync(movimientos_punto movimiento)
        {
            await _context.movimientos_puntos.AddAsync(movimiento);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
