using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Agencias
{
    public class AgenciaPendienteVerificacionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? LogoUrl { get; set; }
        public string? SitioWeb { get; set; }
        public string? Ciudad { get; set; }
        public string? Pais { get; set; }
        public int TotalAcompanantes { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
