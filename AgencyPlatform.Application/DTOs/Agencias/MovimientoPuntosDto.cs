using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias
{
    public class MovimientoPuntosDto
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public string Tipo { get; set; }
        public string Concepto { get; set; }
        public DateTime Fecha { get; set; }
    }
}
