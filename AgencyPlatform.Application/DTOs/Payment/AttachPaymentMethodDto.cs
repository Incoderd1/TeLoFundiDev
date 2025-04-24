using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payment
{
    public class AttachPaymentMethodDto
    {
        [Required]
        public string CustomerId { get; set; }
        [Required]
        public string PaymentMethodId { get; set; }
    }

}
