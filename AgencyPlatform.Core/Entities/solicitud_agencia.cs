using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class solicitud_agencia
    {
        public int id { get; set; }
        public int acompanante_id { get; set; }
        public int agencia_id { get; set; }
        public string estado { get; set; } = "pendiente"; // 'pendiente', 'aprobada', 'rechazada', 'cancelada'
        public DateTime fecha_solicitud { get; set; } = DateTime.UtcNow;
        public DateTime? fecha_respuesta { get; set; }
        public string motivo_rechazo { get; set; }
        public string motivo_cancelacion { get; set; }
        public string mensaje_solicitud { get; set; }
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual acompanante acompanante { get; set; }
        public virtual agencia agencia { get; set; }
    }
}
