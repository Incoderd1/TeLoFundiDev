using AgencyPlatform.Application.DTOs.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Cupones
{
    public interface IPaqueteCuponService
    {
        Task<List<PaqueteCuponDto>> GetAllActivosAsync();
        Task<PaqueteCuponDto> GetByIdAsync(int id);
    }
}
