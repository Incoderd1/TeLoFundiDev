using AgencyPlatform.Application.DTOs.Acompanantes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Busqueda
{
    public class SearchSuggestionsResultDto
    {
        public string Mensaje { get; set; }

        public List<AcompananteSearchResultDto> ResultadosAlternativos { get; set; } = new List<AcompananteSearchResultDto>();

        public bool FiltrosRelajados { get; set; }

        public AdvancedSearchCriteriaDto CriteriosOriginales { get; set; }

        public AdvancedSearchCriteriaDto CriteriosRelajados { get; set; }
    }
}
