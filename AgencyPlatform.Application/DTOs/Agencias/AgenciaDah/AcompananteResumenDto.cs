using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias.AgenciaDah
{
    public class AcompananteResumenDto
    {
        public int Id { get; set; }
        public string NombrePerfil { get; set; }
        public string FotoUrl { get; set; }
        public int TotalVisitas { get; set; }
        public int TotalContactos { get; set; }
    }
}
