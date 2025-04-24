using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Shared.Exporting.Models
{
    public class SolicitudAgenciaExportDto
    {
        public int Id { get; set; }
        public string NombreAcompanante { get; set; }
        public string NombreAgencia { get; set; }
        public string Estado { get; set; }
        public string FechaSolicitud { get; set; }
        public string FechaRespuesta { get; set; }
        public string? MotivoRechazo { get; set; }
        public string? MotivoCancelacion { get; set; }
    }
}
