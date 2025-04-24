using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteIndependienteDto
    {
        public int Id { get; set; }
        public string NombrePerfil { get; set; }
        public string Genero { get; set; }
        public int? Edad { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public string FotoUrl { get; set; }
        public decimal? TarifaBase { get; set; }
        public string Moneda { get; set; }
        public bool? EstaVerificado { get; set; }
    }
}
