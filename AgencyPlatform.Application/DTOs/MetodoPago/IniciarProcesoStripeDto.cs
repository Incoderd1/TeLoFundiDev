using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.MetodoPago
{
    public class IniciarProcesoStripeDto
    {
        [Required]
        public int AcompananteId { get; set; }

        [Required]
        public string ReturnUrl { get; set; }
    }
}
