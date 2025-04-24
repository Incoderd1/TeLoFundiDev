using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias
{
    public class AgenciaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }  // Agregar esta propiedad
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? LogoUrl { get; set; }
        public string? SitioWeb { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Pais { get; set; }
        public bool? EstaVerificada { get; set; }
        public DateTime? FechaVerificacion { get; set; }
        public decimal? ComisionPorcentaje { get; set; }  // Agregar esta propiedad
    }

}
