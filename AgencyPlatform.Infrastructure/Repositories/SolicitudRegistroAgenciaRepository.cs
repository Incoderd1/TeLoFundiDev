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
    public class SolicitudRegistroAgenciaRepository : ISolicitudRegistroAgenciaRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public SolicitudRegistroAgenciaRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<solicitud_registro_agencia>> GetAllAsync()
        {
            return await _context.SolicitudesRegistroAgencia.ToListAsync();
        }

        public async Task<solicitud_registro_agencia> GetByIdAsync(int id)
        {
            return await _context.SolicitudesRegistroAgencia.FirstOrDefaultAsync(s => s.id == id);
        }

        public async Task<List<solicitud_registro_agencia>> GetSolicitudesPendientesAsync()
        {
            return await _context.SolicitudesRegistroAgencia
                .Where(s => s.estado == "pendiente")
                .OrderByDescending(s => s.fecha_solicitud)
                .ToListAsync();
        }

        public async Task<List<solicitud_registro_agencia>> GetSolicitudesByEstadoAsync(string estado)
        {
            return await _context.SolicitudesRegistroAgencia
                .Where(s => s.estado == estado)
                .OrderByDescending(s => s.fecha_solicitud)
                .ToListAsync();
        }

        public async Task AddAsync(solicitud_registro_agencia solicitud)
        {
            solicitud.created_at = DateTime.UtcNow;
            solicitud.updated_at = DateTime.UtcNow;
            solicitud.fecha_solicitud = DateTime.UtcNow;
            solicitud.estado = "pendiente";

            await _context.SolicitudesRegistroAgencia.AddAsync(solicitud);
        }

        public async Task UpdateAsync(solicitud_registro_agencia solicitud)
        {
            solicitud.updated_at = DateTime.UtcNow;
            _context.SolicitudesRegistroAgencia.Update(solicitud);
        }

        public async Task DeleteAsync(solicitud_registro_agencia solicitud)
        {
            _context.SolicitudesRegistroAgencia.Remove(solicitud);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
