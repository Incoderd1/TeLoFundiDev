using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payments
{
    public class CreatePaymentIntentResponseDto
    {
        public string ClientSecret { get; set; }
        public int PaqueteId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
