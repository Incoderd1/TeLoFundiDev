using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Verificacion
{
    public class VerificacionLoteDto
    {
        public int AgenciaId { get; set; }
        public List<int> AcompananteIds { get; set; }
        public decimal MontoCobradoUnitario { get; set; }
        public string Observaciones { get; set; }
    }
}
