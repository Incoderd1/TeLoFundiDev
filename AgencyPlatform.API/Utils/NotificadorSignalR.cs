using AgencyPlatform.API.Hubs;
using AgencyPlatform.Application.Interfaces.Utils;
using Microsoft.AspNetCore.SignalR;

namespace AgencyPlatform.API.Utils
{
    public class NotificadorSignalR : INotificadorRealTime
    {
        private readonly IHubContext<NotificacionesHub> _hubContext;
        private readonly ILogger<NotificadorSignalR> _logger;


        public NotificadorSignalR(
            IHubContext<NotificacionesHub> hubContext,
            ILogger<NotificadorSignalR> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        // Método original con manejo de excepciones
        public async Task NotificarUsuarioAsync(int usuarioId, string mensaje)
        {
            try
            {
                await _hubContext.Clients.User(usuarioId.ToString()).SendAsync("RecibirNotificacion", mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación a usuario {UsuarioId}: {Mensaje}", usuarioId, mensaje);
            }
        }

        // Nuevo método con tipo y datos adicionales
        public async Task NotificarUsuarioAsync(int usuarioId, string mensaje, string tipo, object datos = null)
        {
            try
            {
                await _hubContext.Clients.User(usuarioId.ToString()).SendAsync("RecibirNotificacion", new
                {
                    mensaje = mensaje,
                    tipo = tipo,  // "info", "success", "warning", "error"
                    fecha = DateTime.UtcNow,
                    datos = datos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación estructurada a usuario {UsuarioId}: {Mensaje}", usuarioId, mensaje);
            }
        }

        // Nuevo método para notificar a grupos
        public async Task NotificarGrupoAsync(string grupo, string mensaje)
        {
            try
            {
                await _hubContext.Clients.Group(grupo).SendAsync("RecibirNotificacion", mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación al grupo {Grupo}: {Mensaje}", grupo, mensaje);
            }
        }

        // Nuevo método para notificar a grupos con tipo y datos
        public async Task NotificarGrupoAsync(string grupo, string mensaje, string tipo, object datos = null)
        {
            try
            {
                await _hubContext.Clients.Group(grupo).SendAsync("RecibirNotificacion", new
                {
                    mensaje = mensaje,
                    tipo = tipo,
                    fecha = DateTime.UtcNow,
                    datos = datos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación estructurada al grupo {Grupo}: {Mensaje}", grupo, mensaje);
            }
        }
    }
}

