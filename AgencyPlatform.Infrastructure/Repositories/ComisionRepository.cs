using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class ComisionRepository : IComisionRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public ComisionRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(Comision entity)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<Comision> entities)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Comision entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRangeAsync(IEnumerable<Comision> entities)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Comision>> FindAsync(Expression<Func<Comision, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Comision>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Comision>> GetByAgenciaIdAndFechasAsync(int agenciaId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Comisiones
                .Where(c => c.AgenciaId == agenciaId &&
                      c.Fecha >= fechaInicio &&
                      c.Fecha <= fechaFin)
                .ToListAsync();
        }

        public Task<Comision> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Comision> SingleOrDefaultAsync(Expression<Func<Comision, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Comision entity)
        {
            throw new NotImplementedException();
        }
    }

}
