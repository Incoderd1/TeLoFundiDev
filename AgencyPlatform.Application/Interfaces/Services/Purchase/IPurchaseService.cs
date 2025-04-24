using AgencyPlatform.Application.DTOs.Cliente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Purchase
{
    public interface IPurchaseService
    {
        Task<CompraDto> ComprarPaqueteAsync(int clienteId, ComprarPaqueteDto dto);
        Task<List<CompraDto>> GetHistorialComprasAsync(int clienteId);
        Task<List<CuponClienteDto>> ObtenerCuponesDisponiblesAsync(int clienteId);
        Task<bool> UsarCuponAsync(int clienteId, string codigo);
        Task GenerarCuponesPorCompraAsync(int compraId);
    }
}
