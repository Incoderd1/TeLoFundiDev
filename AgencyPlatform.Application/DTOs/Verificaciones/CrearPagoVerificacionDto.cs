using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Verificaciones
{
    public class CrearPagoVerificacionDto
    {
        public int VerificacionId { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; }
        public string ReferenciaPago { get; set; }
    }
}
