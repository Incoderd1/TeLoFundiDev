using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class Comision
    {
        public int Id { get; set; }
        public int AgenciaId { get; set; }
        public decimal Monto { get; set; }
        public string Concepto { get; set; }
        public string Referencia { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relaciones de navegación
        public virtual agencia Agencia { get; set; }
    }
}