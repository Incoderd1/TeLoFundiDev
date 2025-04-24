using AgencyPlatform.Application.DTOs.Visitas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteEstadisticasDto
    {
        public int Id { get; set; }
        public string NombrePerfil { get; set; }
        public int TotalVisitas { get; set; }
        public int TotalContactos { get; set; }
        public long ScoreActividad { get; set; }
        public Dictionary<string, int> ContactosPorTipo { get; set; } = new Dictionary<string, int>();
        public List<VisitaDiaDto> VisitasPorDia { get; set; } = new List<VisitaDiaDto>();
    }
}
