using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Notificaciones
{
    public interface INotificacionService
    {
        Task NotificarPorEmail(string email, string asunto, string mensaje);
        Task NotificarPorSignalR(int usuarioId, string mensaje, string tipo);
    }
}
