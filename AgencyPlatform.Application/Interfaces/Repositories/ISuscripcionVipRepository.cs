using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface ISuscripcionVipRepository
    {
        Task<suscripciones_vip> GetActivaByClienteIdAsync(int clienteId);
        Task<List<suscripciones_vip>> GetByClienteIdAsync(int clienteId);
        Task AddAsync(suscripciones_vip suscripcion);
        Task UpdateAsync(suscripciones_vip suscripcion);
        Task<bool> SaveChangesAsync();

        Task<suscripciones_vip> GetByReferenciaAsync(string referencia);

        Task<List<suscripciones_vip>> GetExpiradas(DateTime fecha);
        Task<cliente> GetClienteFromSuscripcion(int suscripcionId);

        Task UpdateClienteAsync(cliente cliente);


        //nuevos
        Task<List<suscripciones_vip>> GetActivasByFechaFinAsync(DateTime now);

        Task<List<suscripciones_vip>> GetSuscripcionesVencidasActivasAsync(DateTime fecha);
        Task<List<suscripciones_vip>> GetSuscripcionesPorRenovarAsync(DateTime fecha);
        Task<List<suscripciones_vip>> GetSuscripcionesVencenEnDiasAsync(int dias);


    }
}
