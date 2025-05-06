using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.DTOs.Sorteos;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Cliente
{
    public interface IClienteService
    {
        Task<ClienteDto> RegistrarClienteAsync(RegistroClienteDto dto);
        Task<ClienteDto> GetByIdAsync(int id);
        Task<ClienteDto> GetByUsuarioIdAsync(int usuarioId);
        Task<ClienteDetailDto> GetClienteDetailAsync(int id);
        Task<ClienteDashboardDto> GetDashboardAsync(int clienteId);
        Task<List<MovimientoPuntosDto>> ObtenerHistorialPuntosAsync(int clienteId, int cantidad = 10);
        Task UpdateAsync(cliente cliente);

        //cupones para los clientes
        Task<List<CuponClienteDto>> ObtenerCuponesDisponiblesAsync(int clienteId);
        Task<bool> UsarCuponAsync(int clienteId, string codigo);

        //Task<MovimientoPuntosDto> OtorgarPuntosPorLoginDiarioAsync(int clienteId);
        Task<bool> RegistrarContactoPerfilAsync(int? clienteId, int acompananteId, string tipoContacto, string ipContacto);
        // Método a añadir en IClienteService
        Task RegistrarVisitaPerfilAsync(int? clienteId, int acompananteId, string ipVisitante, string userAgent);

        // Agregar estos métodos a IClienteService.cs
      
        Task<bool> ParticiparEnSorteoAsync(int clienteId, int sorteoId);
        Task<List<SorteoDto>> GetSorteosParticipandoAsync(int clienteId);
        Task<CompraDto> ComprarPaqueteAsync(int clienteId, ComprarPaqueteDto dto);
        Task<List<CompraDto>> GetHistorialComprasAsync(int clienteId);
        Task<bool> SuscribirseVipAsync(int clienteId, SuscribirseVipDto dto);
        Task<bool> CancelarSuscripcionVipAsync(int clienteId);
        Task<bool> EsClienteVipAsync(int clienteId);
        Task<List<AcompananteCardDto>> GetPerfilesVisitadosRecientementeAsync(int clienteId, int cantidad = 5);
        Task<List<AcompananteCardDto>> GetPerfilesRecomendadosAsync(int clienteId, int cantidad = 5);



    }
}
