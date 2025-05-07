using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.MetodoPago
{
    public class BalanceDto
    {
        public decimal Disponible { get; set; }
        public decimal Pendiente { get; set; }
        public string Moneda { get; set; }
    }
}
