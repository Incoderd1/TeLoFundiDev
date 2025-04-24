using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class SuscribirseVipDto
    {
        public int MembresiaId { get; set; }
        public string MetodoPago { get; set; }
        public string ReferenciaPago { get; set; }
        public string PaymentMethodId { get; set; }
        public bool RenovacionAutomatica { get; set; } = true;
    }
}
