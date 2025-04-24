using AgencyPlatform.Application.DTOs.Verificaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.PagoVerificacion
{
    public interface IPagoVerificacionService
    {
        Task<List<PagoVerificacionDto>> GetPagosByAgenciaIdAsync(int agenciaId);
        Task<List<PagoVerificacionDto>> GetPagosPendientesByAgenciaIdAsync(int agenciaId);
        Task<PagoVerificacionDto> GetPagoByIdAsync(int pagoId);
        Task ConfirmarPagoAsync(int pagoId, string referenciaPago);
        Task CancelarPagoAsync(int pagoId, string motivo);
    }
}
