using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IBusquedaRepository
    {
        Task<int> SaveSearchAsync(busqueda_guardada busqueda);
        Task<List<busqueda_guardada>> GetByUsuarioIdAsync(int usuarioId);
        Task<busqueda_guardada?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<bool> UpdateLastUsedAsync(int id);
        Task LogSearchAsync(registro_busqueda registro);
        Task<List<dynamic>> GetPopularSearchTermsAsync(int count);
        Task<int> CountAsync();
        Task SaveChangesAsync();

        Task<List<string>> GetAutocompleteSuggestionsAsync(string prefix, string tipo, int count = 10);

        Task<List<AutocompleteSuggestion>> GetDetailedAutocompleteSuggestionsAsync(string prefix, string tipo, int count = 10);


    }
}
