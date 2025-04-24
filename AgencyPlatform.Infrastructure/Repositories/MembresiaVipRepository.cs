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
    public class MembresiaVipRepository : IMembresiaVipRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public MembresiaVipRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<membresias_vip> GetByIdAsync(int id)
        {
            return await _context.membresias_vips
                .FirstOrDefaultAsync(m => m.id == id);
        }

        public async Task<List<membresias_vip>> GetAllActivasAsync()
        {
            return await _context.membresias_vips
                .Where(m => m.esta_activa == true)
                .OrderBy(m => m.precio_mensual)
                .ToListAsync();
        }
    }
}
