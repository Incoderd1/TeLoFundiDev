using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.SolicitudesAgencia
{
    public class SolicitudAgenciaDto
    {
        public int Id { get; set; }
        public int AcompananteId { get; set; }
        public int AgenciaId { get; set; }
        public string Estado { get; set; } = "pendiente";
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }
    }
}
