using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payments
{
    public class CreatePaymentIntentRequestDto
    {
        public int PaqueteId { get; set; }
        public string Currency { get; set; }
    }
}
