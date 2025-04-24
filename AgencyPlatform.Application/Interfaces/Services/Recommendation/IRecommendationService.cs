using AgencyPlatform.Application.DTOs.Acompanantes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Recommendation
{
    public interface IRecommendationService
    {
        Task<List<AcompananteCardDto>> GetPerfilesVisitadosRecientementeAsync(int clienteId, int cantidad = 5);
        Task<List<AcompananteCardDto>> GetPerfilesRecomendadosAsync(int clienteId, int cantidad = 5);
        Task<List<AcompananteCardDto>> GetPerfilesPopularesAsync(int cantidad = 5);
    }
}
