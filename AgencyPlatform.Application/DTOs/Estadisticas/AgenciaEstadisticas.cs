using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Estadisticas
{
    public class AgenciaEstadisticas
    {
        public int TotalAcompanantes { get; set; }
        public int AcompanantesVerificados { get; set; }
        public long TotalVisitas { get; set; }
        public long TotalContactos { get; set; }
        public decimal IngresosAnuncios { get; set; }
    }
}
