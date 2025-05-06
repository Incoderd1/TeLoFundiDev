using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.EmailAgencia
{
    public interface IEmailProfesionalService
    {
        // Correos relacionados con Agencias
        Task EnviarCorreoAprobacionAgencia(string email, string nombreAgencia);
        Task EnviarCorreoRechazoAgencia(string email, string nombreAgencia, string motivo);
        Task EnviarCorreoVerificacionAgencia(string email, string nombreAgencia);

        // Correos relacionados con Acompañantes
        Task EnviarCorreoVerificacionAcompanante(string email, string nombreAcompanante, string nombreAgencia);
        Task EnviarCorreoSolicitudAprobada(string email, string nombreAcompanante, string nombreAgencia);
        Task EnviarCorreoSolicitudRechazada(string email, string nombreAcompanante, string nombreAgencia, string motivo);
        Task EnviarCorreoInvitacionAgencia(string email, string nombreAcompanante, string nombreAgencia);

        // Correos relacionados con Pagos
        Task EnviarConfirmacionPago(string email, string nombreUsuario, string conceptoPago, decimal monto, string referencia);

        // Correos relacionados con Cupones y Puntos
        Task EnviarConfirmacionCompraPaquete(string email, string nombreCliente, string nombrePaquete, decimal monto, int puntos, List<string> cupones);

        // Correos relacionados con Membresías VIP
        Task EnviarConfirmacionSuscripcionVIP(string email, string nombreCliente, string nombreMembresia, decimal precio, string fechaRenovacion);

        // Correos de sistema
        Task EnviarNotificacionAdministrador(string email, string asunto, string mensaje);
    }
}
