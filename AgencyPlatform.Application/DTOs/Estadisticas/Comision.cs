using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Estadisticas
{
    public class Comision
    {
        public int AcompananteId { get; set; }
        public string NombrePerfil { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
    }
}
