using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Categoria
{
    public class CategoriaEstadisticasDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int TotalAcompanantes { get; set; }
        public int TotalVisitas { get; set; }
    }
}
