using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Anuncios
{
    public class CrearAnuncioDestacadoDto
    {
        public int AgenciaId { get; set; }
        public int AcompananteId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }
        public int? CuponId { get; set; }
    }
}
