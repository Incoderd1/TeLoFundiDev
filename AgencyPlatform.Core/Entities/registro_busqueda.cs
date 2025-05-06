using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class registro_busqueda
    {
        public int id { get; set; }
        public int? usuario_id { get; set; }
        public DateTime fecha_busqueda { get; set; }
        public string criterios_json { get; set; }
        public int cantidad_resultados { get; set; }
        public string ip_busqueda { get; set; }
        public string user_agent { get; set; }

        // Propiedades de navegación
        public virtual usuario usuario { get; set; }
    }
}
