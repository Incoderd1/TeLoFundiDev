using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payment
{
    public class SetDefaultPaymentMethodDto
    {
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
