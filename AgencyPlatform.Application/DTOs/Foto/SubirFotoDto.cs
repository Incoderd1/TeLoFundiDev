using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Foto
{
    public class SubirFotoDto
    {
        [Required(ErrorMessage = "El ID del acompañante es obligatorio")]
        public int AcompananteId { get; set; }

        [Required(ErrorMessage = "El archivo de la foto es obligatorio")]
        public IFormFile Foto { get; set; }

        public bool EsPrincipal { get; set; } = false;

        public int Orden { get; set; } = 0;

        public string Descripcion { get; set; }

        public string Tipo { get; set; } = "perfil"; // Valores posibles: perfil, galeria, verificacion
    }
}
