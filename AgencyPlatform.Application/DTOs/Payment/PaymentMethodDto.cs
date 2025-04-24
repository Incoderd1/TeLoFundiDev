using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payment
{
    public class PaymentMethodDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Brand { get; set; }
        public string Last4 { get; set; }
        public long ExpiryMonth { get; set; }
        public long ExpiryYear { get; set; }
    }
}
