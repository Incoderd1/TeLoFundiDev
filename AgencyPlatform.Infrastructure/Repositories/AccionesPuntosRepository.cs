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
    public class AccionesPuntosRepository : IAccionesPuntosRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public AccionesPuntosRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<acciones_punto> GetByNombreAsync(string nombre)
        {
            return await _context.acciones_puntos
                .FirstOrDefaultAsync(a => a.nombre == nombre);
        }
    }
}
