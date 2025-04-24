using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Usuarios
{
    public class UpdateUserRequest
    {
        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
        public bool EstaActivo { get; set; }
    }

}
