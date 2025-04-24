using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia
{
    public class CrearSolicitudRegistroAgenciaDto
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Ciudad { get; set; } = null!;
        public string Pais { get; set; } = null!;
        public string Direccion { get; set; } = null!;
       

    }
}
