using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.MetodoPago
{
    public class StripeAccountStatusDto
    {
        public string Status { get; set; } // "pending", "active", "rejected", etc.
        public bool PayoutsEnabled { get; set; }
        public bool ChargesEnabled { get; set; }
        public string AccountId { get; set; }
        public string OnboardingUrl { get; set; }
        public bool RequiereAccion { get; set; }
    }
}
