using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias
{
    public class OtorgarPuntosAgenciaDto
    {
        public int AgenciaId { get; set; }
        public int Cantidad { get; set; }
        public string Concepto { get; set; }
    }
}
