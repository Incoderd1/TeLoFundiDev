using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class SolicitudAgencia
    {
        public int Id { get; set; }

        public int AcompananteId { get; set; }
        public int AgenciaId { get; set; }

        public string Estado { get; set; } = "pendiente";
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }

        // Relaciones (opcional, si quieres navegación)
        public virtual acompanante Acompanante { get; set; }
        public virtual agencia Agencia { get; set; }
    }
}
