using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Busqueda
{
    public class SavedSearchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public AdvancedSearchCriteriaDto Criteria { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int ResultCount { get; set; }
        public DateTime LastUsed { get; set; }
    }
}
