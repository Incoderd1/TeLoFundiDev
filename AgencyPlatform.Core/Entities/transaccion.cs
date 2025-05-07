using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class transaccion
    {
        public int id { get; set; }
        public int? cliente_id { get; set; }
        public int? acompanante_id { get; set; }
        public int? agencia_id { get; set; }
        public decimal monto_total { get; set; }
        public decimal monto_acompanante { get; set; }
        public decimal? monto_agencia { get; set; }
        public string estado { get; set; }
        public string proveedor_pago { get; set; }
        public string id_transaccion_externa { get; set; }
        public DateTime fecha_transaccion { get; set; }
        public DateTime? fecha_procesamiento { get; set; }
        public string metadata { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string concepto { get; set; }


        // Navegación
        public virtual cliente cliente { get; set; }
        public virtual acompanante acompanante { get; set; }
        public virtual agencia agencia { get; set; }
    }
}
