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
    public class VerificacionRepository : IVerificacionRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public VerificacionRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<verificacione>> GetAllAsync()
        {
            return await _context.verificaciones
                .Include(v => v.agencia)
            .Include(v => v.acompanante)
                .ToListAsync();
        }

        public async Task<verificacione?> GetByIdAsync(int id)
        {
            return await _context.verificaciones
                .Include(v => v.agencia)
                .Include(v => v.acompanante)
                .FirstOrDefaultAsync(v => v.id == id);
        }

        public async Task<verificacione?> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.verificaciones
                .Include(v => v.agencia)
                .FirstOrDefaultAsync(v => v.acompanante_id == acompananteId);
        }

        public async Task<List<verificacione>> GetByAgenciaIdAsync(int agenciaId)
        {
            return await _context.verificaciones
                .Include(v => v.acompanante)
            .Where(v => v.agencia_id == agenciaId)
            .ToListAsync();
        }

        public async Task<List<verificacione>> GetByAgenciaIdAndPeriodoAsync(int agenciaId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.verificaciones
                .Include(v => v.acompanante)
                .Where(v => v.agencia_id == agenciaId &&
                       v.fecha_verificacion >= fechaInicio &&
                v.fecha_verificacion <= fechaFin)
                .ToListAsync();
        }

        public async Task AddAsync(verificacione entity)
        {
            await _context.verificaciones.AddAsync(entity);
        }

        public async Task UpdateAsync(verificacione entity)
        {
            _context.verificaciones.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(verificacione entity)
        {
            _context.verificaciones.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteByAgenciaIdAsync(int agenciaId)
        {
            var verificaciones = await _context.verificaciones
                .Where(v => v.agencia_id == agenciaId)
                .ToListAsync();

            if (verificaciones.Any())
            {
                _context.verificaciones.RemoveRange(verificaciones);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        // Métodos adicionales implementados para satisfacer la interfaz

        public async Task<List<verificacione>> GetVerificacionesByAgenciaIdAsync(int agenciaId)
        {
            // Este método es básicamente igual a GetByAgenciaIdAsync
            return await GetByAgenciaIdAsync(agenciaId);
        }

        public async Task<List<verificacione>> GetVerificacionesByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.verificaciones
                .Include(v => v.agencia)
                .Include(v => v.acompanante)
                .Where(v => v.fecha_verificacion >= fechaInicio &&
                       v.fecha_verificacion <= fechaFin)
                .ToListAsync();
        }

        public async Task DeleteVerificacionesByAgenciaIdAsync(int agenciaId)
        {
            // Este método es básicamente igual a DeleteByAgenciaIdAsync
            await DeleteByAgenciaIdAsync(agenciaId);
        }
        public async Task<List<verificacione>> GetHistorialVerificacionesAsync(int acompananteId)
        {
            return await _context.verificaciones
                .Where(v => v.acompanante_id == acompananteId)
                .OrderByDescending(v => v.fecha_verificacion)
                .ToListAsync();
        }
    }
}
