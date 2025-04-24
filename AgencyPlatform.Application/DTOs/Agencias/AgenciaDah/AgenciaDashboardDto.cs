using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias.AgenciaDah
{
    public class AgenciaDashboardDto
    {
        public int TotalAcompanantes { get; set; }
        public int TotalVerificados { get; set; }
        public int PendientesVerificacion { get; set; }
        public int SolicitudesPendientes { get; set; }
        public int AnunciosActivos { get; set; }
        public decimal ComisionesUltimoMes { get; set; }
        public int PuntosAcumulados { get; set; }
        public List<AcompananteResumenDto> AcompanantesDestacados { get; set; } = new List<AcompananteResumenDto>();
    }
}
