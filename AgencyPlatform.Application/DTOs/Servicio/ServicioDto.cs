using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Servicio
{
    public class ServicioDto
    {
        public int Id { get; set; }
        public int AcompananteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? DuracionMinutos { get; set; }
    }

}
