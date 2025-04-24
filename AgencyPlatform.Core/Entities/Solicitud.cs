using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class Solicitud
    {
        public int Id { get; set; }
        public int AgenciaId { get; set; }
        public int AcompananteId { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public DateTime? FechaRespuesta { get; set; }
        public string Estado { get; set; } = "pendiente"; // "pendiente", "aprobada", "rechazada", "cancelada"
        public string MotivoRechazo { get; set; }
        public string MensajeSolicitud { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relaciones de navegación
        public virtual agencia Agencia { get; set; }
        public virtual acompanante Acompanante { get; set; }
    }
}
