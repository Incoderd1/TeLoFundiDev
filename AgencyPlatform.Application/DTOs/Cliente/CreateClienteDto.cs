using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class CreateClienteDto
    {
        /// <summary>
        /// Correo electrónico del cliente
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Contraseña en texto plano (luego la encriptamos)
        /// </summary>
        public string Password { get; set; } = null!;

        /// <summary>
        /// Nombre público o apodo del cliente
        /// </summary>
        public string Nickname { get; set; } = null!;
    }
}
