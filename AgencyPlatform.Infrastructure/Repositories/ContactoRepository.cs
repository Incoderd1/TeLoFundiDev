using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class ContactoRepository : IContactoRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public ContactoRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<contacto>> GetByAcompananteIdAsync(int acompananteId)
        {
            return await _context.contactos
                .Where(c => c.acompanante_id == acompananteId)
                .OrderByDescending(c => c.fecha_contacto)
                .ToListAsync();
        }

        public async Task<contacto> GetByIdAsync(int id)
        {
            return await _context.contactos.FindAsync(id);
        }

        public async Task AddAsync(contacto entity)
        {
            await _context.contactos.AddAsync(entity);
        }

        public async Task UpdateAsync(contacto entity)
        {
            _context.contactos.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(contacto entity)
        {
            _context.contactos.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalByAcompananteIdAsync(int acompananteId)
        {
            return await _context.contactos
                .Where(c => c.acompanante_id == acompananteId)
                .CountAsync();
        }

        public async Task<int> GetTotalDesdeAsync(int acompananteId, DateTime fechaInicio)
        {
            return await _context.contactos
                .Where(c => c.acompanante_id == acompananteId && c.fecha_contacto >= fechaInicio)
                .CountAsync();
        }

        public async Task<Dictionary<string, int>> GetContactosPorTipoAsync(int acompananteId)
        {
            var contactosPorTipo = await _context.contactos
                .Where(c => c.acompanante_id == acompananteId)
                .GroupBy(c => c.tipo_contacto)
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            return contactosPorTipo.ToDictionary(c => c.Tipo, c => c.Cantidad);
        }
        public async Task<List<contacto>> GetByClienteIdAsync(int clienteId, int cantidad = 10)
        {
            return await _context.contactos
                .Where(c => c.cliente_id == clienteId)
                .OrderByDescending(c => c.fecha_contacto)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<int> CountByAcompananteIdAsync(int acompananteId)
        {
            return await _context.contactos
                .CountAsync(c => c.acompanante_id == acompananteId);
        }

        // Si necesitas modificar el método SaveChangesAsync existente:
        public async Task<bool> SaveChangesAsync2()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
