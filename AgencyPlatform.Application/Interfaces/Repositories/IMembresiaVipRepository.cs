using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IMembresiaVipRepository
    {
        Task<membresias_vip> GetByIdAsync(int id);
        Task<List<membresias_vip>> GetAllActivasAsync();
    }
}
