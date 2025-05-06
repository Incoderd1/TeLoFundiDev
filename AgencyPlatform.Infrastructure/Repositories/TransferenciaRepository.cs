using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class TransferenciaRepository : ITransferenciaRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public TransferenciaRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<transferencia> GetByIdAsync(int id)
        {
            return await _context.transferencias
                .Include(t => t.transaccion)
                .FirstOrDefaultAsync(t => t.id == id);
        }

        public async Task<List<transferencia>> GetAllAsync()
        {
            return await _context.transferencias
                .Include(t => t.transaccion)
                .ToListAsync();
        }

        public async Task<transferencia> AddAsync(transferencia transferencia)
        {
            await _context.transferencias.AddAsync(transferencia);
            return transferencia;
        }

        public async Task UpdateAsync(transferencia transferencia)
        {
            _context.transferencias.Update(transferencia);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(transferencia transferencia)
        {
            _context.transferencias.Remove(transferencia);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<transferencia>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.transferencias
                .Include(t => t.transaccion)
                .Where(t => t.destino_id == acompananteId && t.destino_tipo == "acompanante")
                .OrderByDescending(t => t.fecha_creacion)
                .ToListAsync();
        }

        public async Task<List<transferencia>> GetByAgenciaIdAsync(int agenciaId)
        {
            return await _context.transferencias
                .Include(t => t.transaccion)
                .Where(t => (t.destino_id == agenciaId && t.destino_tipo == "agencia") ||
                            (t.origen_id == agenciaId && t.origen_tipo == "agencia"))
                .OrderByDescending(t => t.fecha_creacion)
                .ToListAsync();
        }

        public async Task<List<transferencia>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.transferencias
                .Include(t => t.transaccion)
                .Where(t => t.origen_id == clienteId && t.origen_tipo == "cliente")
                .OrderByDescending(t => t.fecha_creacion)
                .ToListAsync();
        }

        public async Task<List<transferencia>> GetByTransaccionIdAsync(int transaccionId)
        {
            return await _context.transferencias
                .Include(t => t.transaccion)
                .Where(t => t.transaccion_id == transaccionId)
                .ToListAsync();
        }

        public async Task<decimal> GetSaldoByUsuarioAsync(int usuarioId, string tipoUsuario)
        {
            var ingresos = await _context.transferencias
                .Where(t => t.destino_id == usuarioId &&
                           t.destino_tipo == tipoUsuario &&
                           t.estado == "completado")
                .SumAsync(t => t.monto);

            var egresos = await _context.transferencias
                .Where(t => t.origen_id == usuarioId &&
                           t.origen_tipo == tipoUsuario &&
                           t.estado == "completado")
                .SumAsync(t => t.monto);

            return ingresos - egresos;
        }
    }
}
