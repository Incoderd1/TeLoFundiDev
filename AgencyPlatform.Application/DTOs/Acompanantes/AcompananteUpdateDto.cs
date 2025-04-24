using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteUpdateDto
    {
        public int Id { get; set; }

        public string NombrePerfil { get; set; }

        public string Genero { get; set; }

        [Range(18, 99)]
        public int? Edad { get; set; }

        public string Descripcion { get; set; }

        public int? Altura { get; set; }

        public int? Peso { get; set; }

        public string Ciudad { get; set; }

        public string Pais { get; set; }

        public string Idiomas { get; set; }

        public string Disponibilidad { get; set; }

        public decimal? TarifaBase { get; set; }

        public string Moneda { get; set; }

        public bool? EstaDisponible { get; set; }
    }
}
