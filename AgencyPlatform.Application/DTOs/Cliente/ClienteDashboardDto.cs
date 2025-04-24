using AgencyPlatform.Application.DTOs.Acompanantes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class ClienteDashboardDto
    {
        public ClienteDto Cliente { get; set; }
        public int PuntosDisponibles { get; set; }
        public int CuponesDisponibles { get; set; }
        public bool TieneMembresiVip { get; set; }
        public DateTime? VencimientoVip { get; set; }
        public List<AcompananteCardDto> PerfilesVisitadosRecentemente { get; set; }
        public List<AcompananteCardDto> PerfilesRecomendados { get; set; }
    }
}
