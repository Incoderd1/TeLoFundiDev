using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Estadisticas
{
    public class AgenciaEstadisticasDto
    {
        public int AgenciaId { get; set; }
        public string NombreAgencia { get; set; } = string.Empty;
        public bool EstaVerificada { get; set; }
        public long TotalAcompanantes { get; set; }         // Cambiado a long
        public long AcompanantesVerificados { get; set; }   // Cambiado a long
        public long AcompanantesDisponibles { get; set; }
    }
}
