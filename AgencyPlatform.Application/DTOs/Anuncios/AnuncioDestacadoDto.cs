using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Anuncios
{
    public class AnuncioDestacadoDto
    {
        public int Id { get; set; }
        public int AcompananteId { get; set; }
        public string NombreAcompanante { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }
        public int? CuponId { get; set; }
        public bool EstaActivo { get; set; }
    }
}
