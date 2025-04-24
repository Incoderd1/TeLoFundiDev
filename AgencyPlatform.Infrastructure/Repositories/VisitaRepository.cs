using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class VisitaRepository : IVisitaRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public VisitaRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<visitas_perfil>> GetAllAsync()
        {
            return await _context.visitas_perfils.ToListAsync();
        }

        public async Task<visitas_perfil?> GetByIdAsync(int id)
        {
            return await _context.visitas_perfils.FirstOrDefaultAsync(v => v.id == id);
        }

        public async Task<List<visitas_perfil>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId)
                .OrderByDescending(v => v.fecha_visita)
                .ToListAsync();
        }

        public async Task<visitas_perfil?> GetVisitaRecienteAsync(int acompananteId, string? ipVisitante)
        {
            if (string.IsNullOrEmpty(ipVisitante))
                return null;

            // Buscar visita del mismo IP en los últimos 5 minutos
            var fechaLimite = DateTime.UtcNow.AddMinutes(-5);
            return await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId &&
                           v.ip_visitante == ipVisitante &&
                           v.fecha_visita >= fechaLimite)
                .OrderByDescending(v => v.fecha_visita)
                .FirstOrDefaultAsync();
        }

        public async Task<long> ContarVisitasTotalesAsync(int acompananteId)
        {
            return await _context.visitas_perfils
                .LongCountAsync(v => v.acompanante_id == acompananteId);
        }

        public async Task<long> ContarVisitasRecientesAsync(int acompananteId, int dias)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-dias);
            return await _context.visitas_perfils
                .LongCountAsync(v => v.acompanante_id == acompananteId && v.fecha_visita >= fechaLimite);
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
        public async Task<List<visitas_perfil>> GetByClienteIdAsync(int clienteId, int cantidad = 10)
        {
            return await _context.visitas_perfils
                .Where(v => v.cliente_id == clienteId)
                .OrderByDescending(v => v.fecha_visita)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<visitas_perfil>> GetByAcompananteIdAsync(int acompananteId, int cantidad = 10)
        {
            return await _context.visitas_perfils
                .Where(v => v.acompanante_id == acompananteId)
                .OrderByDescending(v => v.fecha_visita)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<int> CountByAcompananteIdAsync(int acompananteId)
        {
            return await _context.visitas_perfils
                .CountAsync(v => v.acompanante_id == acompananteId);
        }

      

        public async Task<bool> SaveChangesAsync2()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        // En Infrastructure/Repositories/VisitaRepository.cs
        public async Task<List<int>> GetPerfilesVisitadosRecientementeIdsByClienteAsync(int clienteId, int cantidad)
        {
            return await _context.visitas_perfils
                .Where(v => v.cliente_id == clienteId)
                .OrderByDescending(v => v.fecha_visita)
                .Select(v => v.acompanante_id)
                .Distinct()
                .Take(cantidad)
                .ToListAsync();
        }
    }


}

