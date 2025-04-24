using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payment
{
    public class AttachPaymentMethodResponseDto
    {
        public string PaymentMethodId { get; set; }
        public string Message { get; set; }
    }
}
