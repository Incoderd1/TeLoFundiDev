using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class CuponClienteDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int TipoCuponId { get; set; }
        public string TipoCuponNombre { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public string Codigo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool EstaUsado { get; set; }
        public DateTime? FechaUso { get; set; }
    }
}
