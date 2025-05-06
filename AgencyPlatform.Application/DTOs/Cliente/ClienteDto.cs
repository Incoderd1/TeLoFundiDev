using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public bool EsVip { get; set; }
        public DateTime? FechaInicioVip { get; set; }
        public DateTime? FechaFinVip { get; set; }
        public int PuntosAcumulados { get; set; }
        public string StripeCustomerId { get; set; }

        public List<CuponClienteDto> Cupones { get; set; }
    }
}
