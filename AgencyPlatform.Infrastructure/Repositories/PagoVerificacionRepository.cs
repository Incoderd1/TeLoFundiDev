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
    public class PagoVerificacionRepository : IPagoVerificacionRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public PagoVerificacionRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<pago_verificacion> GetByIdAsync(int id)
        {
            return await _context.PagosVerificacion
                .Include(p => p.verificacion)
                .Include(p => p.acompanante)
                .Include(p => p.agencia)
                .FirstOrDefaultAsync(p => p.id == id);
        }

        public async Task<pago_verificacion> GetByVerificacionIdAsync(int verificacionId)
        {
            return await _context.PagosVerificacion
                .FirstOrDefaultAsync(p => p.verificacion_id == verificacionId);
        }

        public async Task<List<pago_verificacion>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.PagosVerificacion
                .Where(p => p.acompanante_id == acompananteId)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();
        }

        public async Task<List<pago_verificacion>> GetByAgenciaIdAsync(int agenciaId)
        {
            return await _context.PagosVerificacion
                .Where(p => p.agencia_id == agenciaId)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();
        }

        public async Task<List<pago_verificacion>> GetPendientesByAgenciaIdAsync(int agenciaId)
        {
            return await _context.PagosVerificacion
                .Where(p => p.agencia_id == agenciaId && p.estado == "pendiente")
                .OrderByDescending(p => p.created_at)
                .ToListAsync();
        }

        public async Task<bool> ExistenPagosCompletadosAsync(int acompananteId)
        {
            return await _context.PagosVerificacion
                .AnyAsync(p => p.acompanante_id == acompananteId && p.estado == "completado");
        }

        public async Task AddAsync(pago_verificacion pago)
        {
            pago.created_at = DateTime.UtcNow;
            pago.updated_at = DateTime.UtcNow;
            await _context.PagosVerificacion.AddAsync(pago);
        }

        public async Task UpdateAsync(pago_verificacion pago)
        {
            pago.updated_at = DateTime.UtcNow;
            _context.PagosVerificacion.Update(pago);
        }

        public async Task DeleteAsync(pago_verificacion pago)
        {
            _context.PagosVerificacion.Remove(pago);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
