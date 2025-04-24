using AgencyPlatform.Application.DTOs.Cliente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IPuntosService
    {
        Task<MovimientoPuntosDto> OtorgarPuntosPorAccionAsync(int clienteId, string accion);
        Task<MovimientoPuntosDto> OtorgarPuntosPorLoginDiarioAsync(int clienteId);
        Task<MovimientoPuntosDto> OtorgarPuntosManualesAsync(int clienteId, int cantidad, string concepto);
        Task<List<MovimientoPuntosDto>> ObtenerHistorialPuntosAsync(int clienteId, int cantidad = 10);
    }
}
