using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payment
{
    public class CancelSubscriptionDto
    {
        [Required]
        public string SubscriptionId { get; set; }
    }
}
