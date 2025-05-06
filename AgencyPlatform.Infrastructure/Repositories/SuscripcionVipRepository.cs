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
    public class SuscripcionVipRepository : ISuscripcionVipRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public SuscripcionVipRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<suscripciones_vip> GetActivaByClienteIdAsync(int clienteId)
        {
            var ahora = DateTime.UtcNow;
            return await _context.suscripciones_vips
                .Include(s => s.membresia)
                .Where(s => s.cliente_id == clienteId &&
                           s.estado == "activa" &&
                           s.fecha_fin > ahora)
                .OrderByDescending(s => s.fecha_fin)
                .FirstOrDefaultAsync();
        }

        public async Task<List<suscripciones_vip>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.suscripciones_vips
                .Include(s => s.membresia)
                .Where(s => s.cliente_id == clienteId)
                .OrderByDescending(s => s.fecha_inicio)
                .ToListAsync();
        }

        public async Task AddAsync(suscripciones_vip suscripcion)
        {
            await _context.suscripciones_vips.AddAsync(suscripcion);
        }

        public async Task UpdateAsync(suscripciones_vip suscripcion)
        {
            _context.suscripciones_vips.Update(suscripcion);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<suscripciones_vip> GetByReferenciaAsync(string referencia)
        {
            return await _context.suscripciones_vips
                .FirstOrDefaultAsync(s => s.referencia_pago == referencia);
        }
        public async Task<List<suscripciones_vip>> GetExpiradas(DateTime fecha)
        {
            return await _context.suscripciones_vips
                .Where(s => s.estado == "activa" && s.fecha_fin < fecha)
                .ToListAsync();
        }

        public async Task<cliente> GetClienteFromSuscripcion(int suscripcionId)
        {
            var suscripcion = await _context.suscripciones_vips
                .FirstOrDefaultAsync(s => s.id == suscripcionId);

            if (suscripcion == null)
                return null;

            return await _context.clientes
                .FirstOrDefaultAsync(c => c.id == suscripcion.cliente_id);
        }
        public async Task UpdateClienteAsync(cliente cliente)
        {
            _context.clientes.Update(cliente);
        }

        public async Task<List<suscripciones_vip>> GetActivasByFechaFinAsync(DateTime now)
        {
            return await _context.suscripciones_vips
                             .Where(s => s.estado == "activa"
                                         && s.fecha_fin < now)
                             .ToListAsync();
        }
        public async Task<List<suscripciones_vip>> GetExpiradosAsync(DateTime now)
        {
            return await _context.suscripciones_vips
                .Where(s => s.fecha_fin < now && s.estado == "activa")
                .ToListAsync();
        }

        public async Task<List<suscripciones_vip>> GetSuscripcionesVencidasActivasAsync(DateTime fecha)
        {
            return await _context.suscripciones_vips
                .Where(s => s.estado == "activa" && s.fecha_fin < fecha)
                .Include(s => s.cliente)
                    .ThenInclude(c => c.usuario)
                .Include(s => s.membresia)
                .ToListAsync();
        }
        public async Task<List<suscripciones_vip>> GetSuscripcionesPorRenovarAsync(DateTime fecha)
        {
            return await _context.suscripciones_vips
                .Where(s => s.estado == "activa" &&
                           s.fecha_fin < fecha &&
                           s.es_renovacion_automatica == true)
                .Include(s => s.cliente)
                    .ThenInclude(c => c.usuario)
                .Include(s => s.membresia)
                .ToListAsync();
        }
        public async Task<List<suscripciones_vip>> GetSuscripcionesVencenEnDiasAsync(int dias)
        {
            var fechaLimiteInferior = DateTime.UtcNow;
            var fechaLimiteSuperior = DateTime.UtcNow.AddDays(dias);

            return await _context.suscripciones_vips
                .Where(s => s.estado == "activa" &&
                           s.fecha_fin >= fechaLimiteInferior &&
                           s.fecha_fin <= fechaLimiteSuperior)
                .Include(s => s.cliente)
                    .ThenInclude(c => c.usuario)
                .Include(s => s.membresia)
                .ToListAsync();
        }


    }
}
