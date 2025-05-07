using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.MetodoPago
{
    public class TransaccionPagoDto
    {
        public string Id { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Concepto { get; set; }
    }
}
