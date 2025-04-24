using AgencyPlatform.Application.DTOs.Agencias.AgenciaDah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompanantesIndependientesResponseDto
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<AcompananteIndependienteDto> Items { get; set; } = new List<AcompananteIndependienteDto>();
    }
}
