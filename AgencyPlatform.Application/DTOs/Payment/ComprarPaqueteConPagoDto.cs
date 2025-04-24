using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payment
{
    public class ComprarPaqueteConPagoDto
    {
        [Required]
        public int PaqueteId { get; set; }

        [Required]
        public string PaymentIntentId { get; set; }
    }
}
