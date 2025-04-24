using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class SolicitudAgenciaRepository : ISolicitudAgenciaRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public SolicitudAgenciaRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<solicitud_agencia> GetByIdAsync(int id)
        {
            return await _context.solicitud_agencias
                .Include(s => s.agencia)
                .Include(s => s.acompanante)
                .FirstOrDefaultAsync(s => s.id == id);
        }

        public async Task<List<solicitud_agencia>> GetAllAsync()
        {
            return await _context.solicitud_agencias
                .Include(s => s.agencia)
                .Include(s => s.acompanante)
                .ToListAsync();
        }

        public async Task<List<solicitud_agencia>> GetPendientesByAgenciaIdAsync(int agenciaId)
        {
            return await _context.solicitud_agencias
                .Include(s => s.acompanante)
                    .ThenInclude(a => a.fotos)
                .Where(s => s.agencia_id == agenciaId && s.estado == "pendiente")
                .OrderByDescending(s => s.fecha_solicitud)
                .ToListAsync();
        }

        public async Task<List<solicitud_agencia>> GetPendientesByAcompananteIdAsync(int acompananteId)
        {
            return await _context.solicitud_agencias
                .Include(s => s.agencia)
                .Where(s => s.acompanante_id == acompananteId && s.estado == "pendiente")
                .OrderByDescending(s => s.fecha_solicitud)
                .ToListAsync();
        }

        public async Task<int> CountPendientesByAgenciaIdAsync(int agenciaId)
        {
            return await _context.solicitud_agencias
                .Where(s => s.agencia_id == agenciaId && s.estado == "pendiente")
                .CountAsync();
        }

        public async Task<PaginatedResult<solicitud_agencia>> GetHistorialAsync(
            int? agenciaId = null,
            int? acompananteId = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string estado = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // Consulta base
            var query = _context.solicitud_agencias
                .Include(s => s.agencia)
                .Include(s => s.acompanante)
                    .ThenInclude(a => a.fotos)
                .AsQueryable();

            // Aplicar filtros
            if (agenciaId.HasValue)
                query = query.Where(s => s.agencia_id == agenciaId.Value);

            if (acompananteId.HasValue)
                query = query.Where(s => s.acompanante_id == acompananteId.Value);

            if (fechaDesde.HasValue)
                query = query.Where(s => s.fecha_solicitud >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(s => s.fecha_solicitud <= fechaHasta.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(s => s.estado == estado);

            // Ordenar por fecha descendente (más recientes primero)
            query = query.OrderByDescending(s => s.fecha_solicitud);

            // Calcular total y páginas
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Aplicar paginación
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<solicitud_agencia>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<bool> ExistePendienteAsync(int agenciaId, int acompananteId)
        {
            return await _context.solicitud_agencias
                .AnyAsync(s => s.agencia_id == agenciaId &&
                          s.acompanante_id == acompananteId &&
                          s.estado == "pendiente");
        }

        public async Task AddAsync(solicitud_agencia solicitud)
        {
            await _context.solicitud_agencias.AddAsync(solicitud);
        }

        public async Task UpdateAsync(solicitud_agencia solicitud)
        {
            _context.solicitud_agencias.Update(solicitud);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}