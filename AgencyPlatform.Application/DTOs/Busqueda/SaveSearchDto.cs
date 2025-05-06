using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Busqueda
{
    public class SaveSearchDto
    {
        /// <summary>
        /// Nombre para identificar la búsqueda guardada
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Criterios de búsqueda a guardar
        /// </summary>
        public AdvancedSearchCriteriaDto Criteria { get; set; } = new();
    }
}
