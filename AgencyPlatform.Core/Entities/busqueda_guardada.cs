using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class busqueda_guardada
    {
        public int id { get; set; }
        public int usuario_id { get; set; }
        public string nombre { get; set; }
        public string criterios_json { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime? fecha_ultimo_uso { get; set; }
        public int veces_usada { get; set; } = 1;
        public int? cantidad_resultados { get; set; }

        // Propiedades de navegación
        public virtual usuario usuario { get; set; }
    }
}
