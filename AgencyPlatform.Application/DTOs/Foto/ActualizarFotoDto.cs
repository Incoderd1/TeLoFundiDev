using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Foto
{
    public class ActualizarFotoDto
    {
        [Required(ErrorMessage = "El ID de la foto es obligatorio")]
        public int Id { get; set; }

        public bool? EsPrincipal { get; set; }

        public int? Orden { get; set; }

        public string Descripcion { get; set; }

        public string Estado { get; set; }
    }
}
