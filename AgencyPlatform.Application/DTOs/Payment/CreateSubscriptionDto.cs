using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payment
{
    public class CreateSubscriptionDto
    {
        public int? ClienteId { get; set; }
        [Required]
        public int MembresiaId { get; set; }
        [Required]
        public string PaymentMethodId { get; set; }
    }
}
