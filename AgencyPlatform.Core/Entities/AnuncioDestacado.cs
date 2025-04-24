using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class AnuncioDestacado
    {
        public int Id { get; set; }
        public int AcompananteId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; } // "premium", "top", "featured"
        public decimal MontoPagado { get; set; }
        public int? CuponId { get; set; }
        public bool EstaActivo { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relaciones de navegación
        public virtual acompanante Acompanante { get; set; }
    }
}
