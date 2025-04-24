using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Acompanantes.RegistroAcompananate
{
    public class RegisterAcompananteSimpleRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string NombrePerfil { get; set; }

        [Required]
        public string Genero { get; set; }

        [Required]
        [Range(18, 99)]
        public int Edad { get; set; }

        public string? Ciudad { get; set; }

        public string? Pais { get; set; }

        public string? WhatsApp { get; set; }
    }
}
