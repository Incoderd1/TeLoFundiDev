using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.Notificaciones;
using AgencyPlatform.Application.Interfaces.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Notificaciones
{
    public class NotificacionService : INotificacionService
    {
        private readonly IEmailSender _emailSender;
        private readonly INotificadorRealTime _notificador;

        public NotificacionService(IEmailSender emailSender, INotificadorRealTime notificador)
        {
            _emailSender = emailSender;
            _notificador = notificador;
        }

        public async Task NotificarPorEmail(string email, string asunto, string mensaje)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
            }
        }

        public async Task NotificarPorSignalR(int usuarioId, string mensaje, string tipo)
        {
            await _notificador.NotificarUsuarioAsync(usuarioId, mensaje, tipo);
        }
    }
}