using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Busqueda
{
    public class SearchSuggestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Ciudad, Categoria, Servicio, etc.
        public int ResultCount { get; set; }
        public int UsageCount { get; set; }
    }
}
