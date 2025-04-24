using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Verificaciones
{
    public class ActualizarPagoVerificacionDto
    {
        public int Id { get; set; }
        public string Estado { get; set; }
        public string ReferenciaPago { get; set; }
    }
}
