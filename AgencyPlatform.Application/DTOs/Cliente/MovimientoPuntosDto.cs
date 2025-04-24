using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Cliente
{
    public class MovimientoPuntosDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int Cantidad { get; set; }
        public string Tipo { get; set; } // "ingreso" o "egreso"
        public string Concepto { get; set; }
        public int SaldoAnterior { get; set; }
        public int SaldoNuevo { get; set; }
        public DateTime Fecha { get; set; }
    }
}
