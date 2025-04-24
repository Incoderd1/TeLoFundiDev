using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Usuarios
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public bool EstaActivo { get; set; }
    }
}
