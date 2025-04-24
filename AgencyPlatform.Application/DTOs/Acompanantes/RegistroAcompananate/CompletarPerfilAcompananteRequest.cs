using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes.RegistroAcompananate
{
    public class CompletarPerfilAcompananteRequest
    {
        [Required]
        public string Descripcion { get; set; }

        [Required]
        public decimal TarifaBase { get; set; }

        [Required]
        public string Disponibilidad { get; set; }

        public int? Altura { get; set; }

        public int? Peso { get; set; }

        public string? Idiomas { get; set; }

        [Required]
        public List<int> CategoriaIds { get; set; }

        public IFormFile? FotoPrincipal { get; set; }

        public List<IFormFile>? FotosAdicionales { get; set; }

        public int? SolicitarAgenciaId { get; set; }
    }
}
