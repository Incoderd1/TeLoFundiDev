using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class ComprarPaqueteDto
    {
        public int PaqueteId { get; set; }
        public string MetodoPago { get; set; }
        public string ReferenciaPago { get; set; }
        public string PaymentIntentId { get; set; }
    }
}
