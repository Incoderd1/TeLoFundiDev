using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class SolicitudRepository : ISolicitudRepository
    {
        private readonly AgencyPlatformDbContext _context; // Usamos AgencyPlatformDbContext
        //private readonly DbSet<solicitud_agencia> _solicitudes;
        private readonly ILogger<SolicitudRepository> _logger;

        public SolicitudRepository(AgencyPlatformDbContext context, ILogger<SolicitudRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<solicitud_agencia> GetByIdAsync(int solicitudId)
        {
            _logger.LogInformation("Obteniendo solicitud con ID: {SolicitudId}", solicitudId);
            var solicitud = await _context.solicitud_agencias.FindAsync(solicitudId);
            if (solicitud == null)
            {
                _logger.LogWarning("Solicitud con ID: {SolicitudId} no encontrada", solicitudId);
            }
            else
            {
                _logger.LogInformation("Solicitud con ID: {SolicitudId} obtenida correctamente", solicitudId);
            }
            return solicitud;
        }

        public async Task AddAsync(solicitud_agencia solicitud)
        {
            _logger.LogInformation("Agregando nueva solicitud: AcompananteId={AcompananteId}, AgenciaId={AgenciaId}", solicitud.acompanante_id, solicitud.agencia_id);
            await _context.solicitud_agencias.AddAsync(solicitud);
            await SaveChangesAsync();
            _logger.LogInformation("Solicitud con ID: {SolicitudId} agregada correctamente", solicitud.id);
        }

        public async Task UpdateAsync(solicitud_agencia solicitud)
        {
            _logger.LogInformation("Actualizando solicitud con ID: {SolicitudId}", solicitud.id);
            _context.solicitud_agencias.Update(solicitud);
            await SaveChangesAsync();
            _logger.LogInformation("Solicitud con ID: {SolicitudId} actualizada correctamente", solicitud.id);
        }

        public async Task DeleteAsync(solicitud_agencia solicitud)
        {
            _logger.LogInformation("Eliminando solicitud con ID: {SolicitudId}", solicitud.id);
            _context.solicitud_agencias.Remove(solicitud);
            await SaveChangesAsync();
            _logger.LogInformation("Solicitud con ID: {SolicitudId} eliminada correctamente", solicitud.id);
        }

        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Guardando cambios en la base de datos");
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cambios guardados exitosamente");
        }
    }
}
