using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payments
{
    public class DetallePaqueteDto
    {
        public int TipoCuponId { get; set; }
        public string NombreCupon { get; set; }
        public int PorcentajeDescuento { get; set; }
        public int Cantidad { get; set; }
    }
}
