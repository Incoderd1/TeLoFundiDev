using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias
{
    public class HorarioDto
    {
        public Dictionary<string, string>? Dias { get; set; }  // Ej: { "lunes": "10:00-18:00", "martes": "cerrado", ... }

    }
}
