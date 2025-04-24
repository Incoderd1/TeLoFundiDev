using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Foto
{
    public class FotoDto
    {
        public int Id { get; set; }
        public int AcompananteId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }

        // ← Nuevos campos
        public bool Verificada { get; set; }
        public DateTime FechaVerificacion { get; set; }
    }
}
