using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    // DTO sin los campos de mostrar
    public class AcompananteCreateDto
    {
        [Required]
        public string NombrePerfil { get; set; }

        [Required]
        public string Genero { get; set; }

        [Range(18, 99)]
        public int Edad { get; set; }

        public string Descripcion { get; set; }

        public int? Altura { get; set; }

        public int? Peso { get; set; }

        public string Ciudad { get; set; }

        public string Pais { get; set; }

        public string Idiomas { get; set; }

        public string Disponibilidad { get; set; }

        public decimal? TarifaBase { get; set; }

        public string Moneda { get; set; } = "USD";

        public bool EstaDisponible { get; set; } = true;

        // Solo los campos de contacto
        [Phone]
        public string Telefono { get; set; }

        [Phone]
        public string WhatsApp { get; set; }

        [EmailAddress]
        public string EmailContacto { get; set; }

        public List<int> CategoriaIds { get; set; } = new List<int>();
    }
}

