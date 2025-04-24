using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Usuarios
{
    public class UpdateUsuarioDto
    {
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool? EstaActivo { get; set; }
        public string? TipoUsuario { get; set; } // cliente, agencia, etc.
    }
}
