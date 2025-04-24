using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.DTOs.Servicio;
using AgencyPlatform.Application.DTOs.Categoria;

namespace AgencyPlatform.Application.DTOs.Acompanantes
{
    public class AcompananteDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? AgenciaId { get; set; }
        public string? NombreAgencia { get; set; }
        public string NombrePerfil { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string? Descripcion { get; set; }
        public int? Altura { get; set; }
        public int? Peso { get; set; }
        public string? Ciudad { get; set; }
        public string? Pais { get; set; }
        public string? Idiomas { get; set; }
        public string? Disponibilidad { get; set; }
        public decimal? TarifaBase { get; set; }
        public string Moneda { get; set; } = "USD";
        public bool EstaVerificado { get; set; }
        public DateTime? FechaVerificacion { get; set; }
        public bool EstaDisponible { get; set; }
        public string? FotoPrincipal { get; set; }
        public List<FotoDto> Fotos { get; set; } = new List<FotoDto>();
        public List<ServicioDto> Servicios { get; set; } = new List<ServicioDto>();
        public List<CategoriaDto> Categorias { get; set; } = new List<CategoriaDto>();
    }
}
