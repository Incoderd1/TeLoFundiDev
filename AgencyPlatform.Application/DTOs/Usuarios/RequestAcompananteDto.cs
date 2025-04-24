using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Usuarios
{
    public class RequestAcompananteDto
    {
        public string NombrePerfil { get; set; }
        public string Descripcion { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public string Idiomas { get; set; }
        public string Disponibilidad { get; set; }
        public decimal TarifaBase { get; set; }
        public string Moneda { get; set; }
        public bool EsIndependiente { get; set; } // Define si el acompañante es independiente o asociado a una agencia
        public int? AgenciaId { get; set; } // Si no es independiente, debe seleccionar una agencia
    }
}
