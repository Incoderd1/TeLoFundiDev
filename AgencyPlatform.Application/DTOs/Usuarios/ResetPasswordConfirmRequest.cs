using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Usuarios
{
    public class ResetPasswordConfirmRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
