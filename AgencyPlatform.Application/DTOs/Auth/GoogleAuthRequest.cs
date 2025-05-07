using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Auth
{
    public class GoogleAuthRequest
    {
        public string IdToken { get; set; }
        public string NombrePerfil { get; set; }
    }
}
