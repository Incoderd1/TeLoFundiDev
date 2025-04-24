using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Estadisticas
{
    public class ComisionesDto
    {
        public int AgenciaId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal PorcentajeComision { get; set; }
        public int TotalVerificaciones { get; set; }
        public decimal MontoTotalVerificaciones { get; set; }
        public decimal ComisionTotal { get; set; }
    }
}
