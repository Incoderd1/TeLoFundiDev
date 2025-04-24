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
    public class VisitaPerfilRepository : IVisitaPerfilRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public VisitaPerfilRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<visitas_perfil>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId)
                .OrderByDescending(v => v.fecha_visita)
                .ToListAsync();
        }

        public async Task<visitas_perfil> GetByIdAsync(int id)
        {
            return await _context.visitas_perfils.FindAsync(id);
        }

        public async Task AddAsync(visitas_perfil entity)
        {
            await _context.visitas_perfils.AddAsync(entity);
        }

        public async Task UpdateAsync(visitas_perfil entity)
        {
            _context.visitas_perfils.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(visitas_perfil entity)
        {
            _context.visitas_perfils.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalByAcompananteIdAsync(int acompananteId)
        {
            return await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId)
                .CountAsync();
        }

        public async Task<int> GetTotalDesdeAsync(int acompananteId, DateTime fechaInicio)
        {
            return await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId && v.fecha_visita >= fechaInicio)
                .CountAsync();
        }

        public async Task<Dictionary<DateTime, int>> GetVisitasPorDiaAsync(int acompananteId, int dias)
        {
            var fechaInicio = DateTime.UtcNow.Date.AddDays(-dias);

            // Obtener todas las visitas sin agrupar
            var todasLasVisitas = await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId && v.fecha_visita >= fechaInicio)
                .Select(v => new { Fecha = v.fecha_visita })
                .ToListAsync();

            // Agrupar en memoria
            var resultado = new Dictionary<DateTime, int>();
            for (int i = 0; i <= dias; i++)
            {
                var fecha = fechaInicio.AddDays(i).Date;
                var cantidadVisitas = todasLasVisitas.Count(v => v.Fecha.HasValue && v.Fecha.Value.Date == fecha);
                resultado.Add(fecha, cantidadVisitas);
            }

            return resultado;
        }
    }
}
