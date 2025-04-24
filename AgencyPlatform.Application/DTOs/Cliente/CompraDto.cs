using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class CompraDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PaqueteId { get; set; }
        public string PaqueteNombre { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal MontoPagado { get; set; }
        public string MetodoPago { get; set; }
        public string ReferenciaPago { get; set; }
        public string Estado { get; set; }
    }
}
