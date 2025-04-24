using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class RegistrarVisitaDto
    {
        public int? ClienteId { get; set; }
        public int AcompananteId { get; set; }
    }
}
