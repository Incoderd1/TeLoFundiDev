using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Verificaciones
{
    public class PagoVerificacionDto
    {
        public int Id { get; set; }
        public int VerificacionId { get; set; }
        public int AcompananteId { get; set; }
        public int AgenciaId { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; }
        public string MetodoPago { get; set; }
        public string ReferenciaPago { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaPago { get; set; }
    }

}
