using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payments
{
    public class PaqueteCuponDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int PuntosOtorgados { get; set; }
        public bool IncluyeSorteo { get; set; }
        public List<DetallePaqueteDto> Detalles { get; set; }
    }
}
