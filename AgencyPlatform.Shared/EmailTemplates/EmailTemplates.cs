using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Shared.EmailTemplates
{
    public static class EmailTemplates
    {
        public static string SolicitudAprobadaAgencia(string nombreAgencia, string nombreAcompanante)
        {
            return $@"
                Hola {nombreAgencia},

                Has aprobado la solicitud del perfil '{nombreAcompanante}' y ahora forma parte de tu agencia.

                Puedes gestionarlo desde el panel.

                Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
                ";
        }

        public static string SolicitudAprobadaAcompanante(string nombreAcompanante, string nombreAgencia)
        {
            return $@"
                Hola {nombreAcompanante},

                ¡Tu solicitud para unirte a la agencia '{nombreAgencia}' ha sido aprobada!

                Ya formas parte de esta agencia. Puedes revisar tus beneficios en el panel.

                Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
                ";
        }

        public static string SolicitudRechazadaAcompanante(string nombreAcompanante, string nombreAgencia)
        {
            return $@"
                Hola {nombreAcompanante},

                Tu solicitud para unirte a la agencia '{nombreAgencia}' fue rechazada.

                Puedes intentar postularte a otra agencia o contactar directamente.

                Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
                ";
        }

        public static string SolicitudCanceladaAcompanante(string nombreAcompanante, string nombreAgencia, string motivo)
        {
            return $@"
                Hola {nombreAcompanante},

                Tu solicitud a la agencia '{nombreAgencia}' ha sido cancelada.

                Motivo: {motivo}

                Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
                ";
                        }

        public static string SolicitudCanceladaAgencia(string nombreAgencia, string nombreAcompanante, string motivo)
        {
            return $@"
                Hola {nombreAgencia},

                La solicitud de {nombreAcompanante} ha sido cancelada.

                Motivo: {motivo}

                Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
                ";
        }

        public static string NuevaSolicitudRecibida(string nombreAgencia, string nombreAcompanante)
        {
                    return $@"
        Hola {nombreAgencia},

        El perfil '{nombreAcompanante}' ha enviado una solicitud para unirse a tu agencia.

        Revisa tus solicitudes pendientes en el panel de administración para gestionarla.

        Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
        ";
                }

        public static string SolicitudRechazadaAgencia(string nombreAgencia, string nombreAcompanante)
        {
                    return $@"
                Hola {nombreAgencia},

                La solicitud de {nombreAcompanante} ha sido rechazada exitosamente.

                Puedes gestionar otras solicitudes desde tu panel.

                Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
            ";
        }
    }
}
