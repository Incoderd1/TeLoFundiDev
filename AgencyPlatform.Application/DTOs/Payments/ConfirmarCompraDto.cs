using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payments
{
    public class ConfirmarCompraDto
    {
        public int PaqueteId { get; set; }
        public string MetodoPago { get; set; } // "stripe", "paypal", etc.
        public string PaymentIntentId { get; set; }
        public string ReferenciaPago { get; set; }
    }
}
