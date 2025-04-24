using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Utils
{
    public interface INotificadorRealTime
    {
        Task NotificarUsuarioAsync(int usuarioId, string mensaje);

        Task NotificarUsuarioAsync(int usuarioId, string mensaje, string tipo, object datos = null);
        Task NotificarGrupoAsync(string grupo, string mensaje);
        Task NotificarGrupoAsync(string grupo, string mensaje, string tipo, object datos = null);
    }
}


