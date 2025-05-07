using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class TransaccionRepository : ITransaccionRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public TransaccionRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<transaccion> GetByIdAsync(int id)
        {
            return await _context.transacciones
                .Include(t => t.cliente)
                .Include(t => t.acompanante)
                .Include(t => t.agencia)
                .FirstOrDefaultAsync(t => t.id == id);
        }

        public async Task<List<transaccion>> GetAllAsync()
        {
            return await _context.transacciones
                .Include(t => t.cliente)
                .Include(t => t.acompanante)
                .Include(t => t.agencia)
                .ToListAsync();
        }

        public async Task<transaccion> AddAsync(transaccion transaccion)
        {
            await _context.transacciones.AddAsync(transaccion);
            return transaccion;
        }

        public async Task UpdateAsync(transaccion transaccion)
        {
            _context.transacciones.Update(transaccion);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(transaccion transaccion)
        {
            _context.transacciones.Remove(transaccion);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<transaccion>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.transacciones
                .Include(t => t.acompanante)
                .Include(t => t.agencia)
                .Where(t => t.cliente_id == clienteId)
                .OrderByDescending(t => t.fecha_transaccion)
                .ToListAsync();
        }

        public async Task<List<transaccion>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.transacciones
                .Include(t => t.cliente)
                .Include(t => t.agencia)
                .Where(t => t.acompanante_id == acompananteId)
                .OrderByDescending(t => t.fecha_transaccion)
                .ToListAsync();
        }

        public async Task<List<transaccion>> GetByAgenciaIdAsync(int agenciaId)
        {
            return await _context.transacciones
                .Include(t => t.cliente)
                .Include(t => t.acompanante)
                .Where(t => t.agencia_id == agenciaId)
                .OrderByDescending(t => t.fecha_transaccion)
                .ToListAsync();
        }

        public async Task<transaccion> GetByExternalIdAsync(string externalId)
        {
            return await _context.transacciones
                .Include(t => t.cliente)
                .Include(t => t.acompanante)
                .Include(t => t.agencia)
                .FirstOrDefaultAsync(t => t.id_transaccion_externa == externalId);
        }

        public async Task<decimal> GetTotalPagadoByClienteIdAsync(int clienteId)
        {
            return await _context.transacciones
                .Where(t => t.cliente_id == clienteId && t.estado == "completado")
                .SumAsync(t => t.monto_total);
        }
        public async Task<List<transaccion>> GetByAcompananteIdAsync(
       int acompananteId,
       DateTime? desde = null,
       DateTime? hasta = null,
       int pagina = 1,
       int elementosPorPagina = 10)
        {
            var query = _context.transacciones
                .Where(t => t.acompanante_id == acompananteId);

            // Aplicar filtros de fecha si se proporcionan
            if (desde.HasValue)
                query = query.Where(t => t.fecha_transaccion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(t => t.fecha_transaccion <= hasta.Value);

            // Aplicar ordenamiento y paginación
            return await query
                .OrderByDescending(t => t.fecha_transaccion)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync();
        }
    }
}
