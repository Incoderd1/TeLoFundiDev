using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Verificaciones
{
    public class VerificacionDto
    {
        public int Id { get; set; }
        public int AgenciaId { get; set; }
        public int AcompananteId { get; set; }
        public DateTime FechaVerificacion { get; set; }
        public decimal? MontoCobrado { get; set; }
        public string Estado { get; set; } = "aprobada";
        public string? Observaciones { get; set; }
    }
}
