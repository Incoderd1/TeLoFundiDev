using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteCardDto
    {
        public int Id { get; set; }
        public string NombrePerfil { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public string Genero { get; set; }
        public decimal TarifaBase { get; set; }
        public string Moneda { get; set; }
        public string FotoPrincipalUrl { get; set; }
        public bool EstaVerificado { get; set; }
        public bool EstaDisponible { get; set; }
        public int TotalVisitas { get; set; }
        public int TotalContactos { get; set; }
        public string RazonRecomendacion { get; set; }
    }
}
