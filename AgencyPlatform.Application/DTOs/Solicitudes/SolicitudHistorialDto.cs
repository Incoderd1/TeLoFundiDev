using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Solicitudes
{
    public class SolicitudHistorialDto
    {
        public int Id { get; set; }
        public int AcompananteId { get; set; }
        public string NombreAcompanante { get; set; }
        public string FotoAcompanante { get; set; }
        public int AgenciaId { get; set; }
        public string NombreAgencia { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string Estado { get; set; } // "pendiente", "aprobada", "rechazada", "cancelada"
        public string MotivoRechazo { get; set; }
        public string MotivoCancelacion { get; set; }
    }
}
