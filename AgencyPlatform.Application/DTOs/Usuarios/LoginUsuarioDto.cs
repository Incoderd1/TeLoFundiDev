using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Usuarios
{
    public class LoginUsuarioDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
