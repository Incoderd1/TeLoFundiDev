using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteSearchResultDto
    {
        public int Id { get; set; }
        public string NombrePerfil { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int? Edad { get; set; }
        public string Ciudad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public decimal? TarifaBase { get; set; }
        public string Moneda { get; set; } = "USD";
        public bool EstaVerificado { get; set; }
        public bool EstaDisponible { get; set; }
        public string? FotoPrincipalUrl { get; set; }
        public int TotalFotos { get; set; }
        public List<string> Categorias { get; set; } = new();
        public List<string> Servicios { get; set; } = new();
        public string? NombreAgencia { get; set; }
        public bool AgenciaVerificada { get; set; }
        public double? RelevanciaScore { get; set; }
        public long ScoreActividad { get; set; }
        public int TotalVisitas { get; set; }
        public int TotalContactos { get; set; }
        public string NotaRecomendacion { get; set; }

        public double Distancia { get; set; }

    }
}
