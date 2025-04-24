using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias
{
    public class PuntosAgenciaDto
    {
        public int AgenciaId { get; set; }
        public int PuntosDisponibles { get; set; }
        public int PuntosGastados { get; set; }
        public List<MovimientoPuntosDto> UltimosMovimientos { get; set; }
    }
}
