using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs
{
    public class RegistrarContactoDto
    {
        public int? ClienteId { get; set; }
        public int AcompananteId { get; set; }
        public string TipoContacto { get; set; } // Puede ser "telefono", "whatsapp", "email", etc.
    }
}
