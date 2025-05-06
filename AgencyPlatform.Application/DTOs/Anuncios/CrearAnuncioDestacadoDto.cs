using AgencyPlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Anuncios
{
    public class CrearAnuncioDestacadoDto
    {
        public int AcompananteId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TipoAnuncio Tipo { get; set; }
        public string MetodoPago { get; set; } // Agregamos la propiedad para el método de pago
    }

}