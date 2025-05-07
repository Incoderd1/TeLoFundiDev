using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class transferencia
    {
        public int id { get; set; }
        public int transaccion_id { get; set; }
        public int? origen_id { get; set; }
        public string origen_tipo { get; set; }
        public int destino_id { get; set; }
        public string destino_tipo { get; set; }
        public decimal monto { get; set; }
        public string estado { get; set; }
        public string proveedor_pago { get; set; }
        public string id_transferencia_externa { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime? fecha_procesamiento { get; set; }
        public string error_mensaje { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public string concepto { get; set; }


        // Navegación
        public virtual transaccion transaccion { get; set; }
    }
}
