using AgencyPlatform.Application.DTOs.Foto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteResumen2Dto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Pais { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? Descripcion { get; set; }
        public string Genero { get; set; } = string.Empty;
        public List<FotoDto> Fotos { get; set; } = new List<FotoDto>();
        public string? Whatsapp { get; set; }
    }
}
