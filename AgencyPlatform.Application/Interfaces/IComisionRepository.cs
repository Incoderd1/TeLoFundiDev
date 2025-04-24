using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces
{
    public interface IComisionRepository : IGenericRepository<Comision>
    {
        Task<List<Comision>> GetByAgenciaIdAndFechasAsync(int agenciaId, DateTime fechaInicio, DateTime fechaFin);
    }
}
