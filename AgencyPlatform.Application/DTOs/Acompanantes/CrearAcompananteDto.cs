using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;




namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class CrearAcompananteDto
    {
        public string NombrePerfil { get; set; }
        public string Genero { get; set; }
        public int Edad { get; set; }
        public string? Descripcion { get; set; }
        public int? Altura { get; set; }
        public int? Peso { get; set; }
        public string? Ciudad { get; set; }
        public string? Pais { get; set; }
        public string? Idiomas { get; set; }
        public string? Disponibilidad { get; set; }
        public decimal? TarifaBase { get; set; }
        public string? Moneda { get; set; }
        public List<int> CategoriaIds { get; set; } = new List<int>();
        public int? AgenciaId { get; set; }

        // Campos nuevos
        public string? Telefono { get; set; }
        public string? WhatsApp { get; set; }
   
        public string? EmailContacto { get; set; }
    }
}
