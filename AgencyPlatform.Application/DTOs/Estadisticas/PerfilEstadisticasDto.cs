using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Estadisticas
{
    public class PerfilEstadisticasDto
    {
        public int AcompananteId { get; set; }
        public string NombrePerfil { get; set; } = null!;
        public string Ciudad { get; set; } = null!;
        public bool EstaVerificado { get; set; }
        public int TotalVisitas { get; set; }
        public int TotalContactos { get; set; }
        public int ScoreActividad { get; set; }


        public string? FotoUrl { get; set; }
        public DateTime? UltimaVisita { get; set; }
    }

}
