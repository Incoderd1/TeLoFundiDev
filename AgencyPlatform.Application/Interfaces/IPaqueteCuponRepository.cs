using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces
{
    public interface IPaqueteCuponRepository
    {
        Task<paquetes_cupone> GetByIdAsync(int id);
        Task<List<paquetes_cupone>> GetActivosAsync();
        Task<List<paquete_cupones_detalle>> GetDetallesByPaqueteIdAsync(int paqueteId);

        Task<List<paquetes_cupone>> GetAllActivosAsync();
       
        Task<tipos_cupone> GetTipoCuponByIdAsync(int tipoCuponId);
    }
}
