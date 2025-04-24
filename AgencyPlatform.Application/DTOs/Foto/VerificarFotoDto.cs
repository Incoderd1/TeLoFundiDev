using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Foto
{
    public class VerificarFotoDto
    {
        [Required(ErrorMessage = "El ID de la foto es obligatorio")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El estado de verificación es obligatorio")]
        public bool Verificada { get; set; }

        public string Observaciones { get; set; }
    }
}
