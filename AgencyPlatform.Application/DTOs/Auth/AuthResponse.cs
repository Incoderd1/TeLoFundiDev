using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int? AcompananteId { get; set; }
        public int? AgenciaId { get; set; }
        public int? ClienteId { get; set; }
        public bool PerfilCompleto { get; set; }
        public string NombrePerfil { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
