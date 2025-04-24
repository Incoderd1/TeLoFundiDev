using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogActivityAsync(string entidad, int entidadId, string accion, string descripcion, int usuarioId);
        Task LogActivityWithDetailsAsync(string entidad, int entidadId, string accion, string descripcion, int usuarioId, Dictionary<string, string> valoresAnteriores, Dictionary<string, string> valoresNuevos);
    }
}
