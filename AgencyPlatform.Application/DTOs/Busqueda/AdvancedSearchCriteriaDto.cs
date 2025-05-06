using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Busqueda
{
    public class AdvancedSearchCriteriaDto
    {
        // Criterios de texto
        public string? SearchText { get; set; }
        public bool? MatchExactPhrase { get; set; }

        // Ubicación
        public string? Ciudad { get; set; }
        public string? Pais { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public int? RadioKm { get; set; }

        // Características físicas
        public string? Genero { get; set; }
        public int? EdadMinima { get; set; }
        public int? EdadMaxima { get; set; }
        public int? AlturaMinima { get; set; }
        public int? AlturaMaxima { get; set; }

        // Aspectos de servicio
        public decimal? TarifaMinima { get; set; }
        public decimal? TarifaMaxima { get; set; }
        public string? Moneda { get; set; }
        public List<int>? CategoriaIds { get; set; }
        public List<int>? ServicioIds { get; set; }
        public List<string>? IdiomasRequeridos { get; set; }

        // Disponibilidad y verificación
        public bool? SoloVerificados { get; set; }
        public bool? SoloDisponibles { get; set; }
        public bool? ConAgencia { get; set; }
        public int? AgenciaId { get; set; }

        // Opciones de multimedia
        public bool? SoloConFotos { get; set; }
        public int? MinimoFotos { get; set; }

        // Paginación
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Ordenamiento
        public string OrderBy { get; set; } = "Relevancia";
        public bool OrderDescending { get; set; } = true;
    }
}
