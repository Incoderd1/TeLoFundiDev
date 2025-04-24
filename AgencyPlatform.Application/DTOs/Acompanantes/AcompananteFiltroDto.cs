using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteFiltroDto
    {
        public string Busqueda { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public string Genero { get; set; }
        public int? EdadMinima { get; set; }
        public int? EdadMaxima { get; set; }
        public decimal? TarifaMinima { get; set; }
        public decimal? TarifaMaxima { get; set; }
        public bool? SoloVerificados { get; set; }
        public bool? SoloDisponibles { get; set; } = true;
        public List<int> CategoriaIds { get; set; } = new List<int>();
        public string OrdenarPor { get; set; } = "recientes";
        public int Pagina { get; set; } = 1;
        public int ElementosPorPagina { get; set; } = 10;
    }
}
