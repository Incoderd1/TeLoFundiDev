using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class MembresiVipDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioMensual { get; set; }
        public int PuntosMensuales { get; set; }
        public int DescuentoAnuncios { get; set; }
        public bool Activa { get; set; }
    }
}
