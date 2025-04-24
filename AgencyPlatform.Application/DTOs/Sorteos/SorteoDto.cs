using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Sorteos
{
    public class SorteoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool EstaActivo { get; set; }
        public bool EstoyParticipando { get; set; }
        public int TotalParticipantes { get; set; }
    }
}
