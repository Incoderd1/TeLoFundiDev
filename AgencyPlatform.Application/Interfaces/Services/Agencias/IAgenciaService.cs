using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Application.DTOs.Verificaciones;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Application.DTOs.Anuncios;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.SolicitudesAgencia;
using AgencyPlatform.Application.DTOs.Agencias.AgenciaDah;
using AgencyPlatform.Application.DTOs.Solicitudes;
using AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia;
using AgencyPlatform.Application.DTOs.Verificacion;
using AgencyPlatform.Application.DTOs;

namespace AgencyPlatform.Application.Interfaces.Services.Agencias
{
    public interface IAgenciaService
    {
        Task<SolicitudAgenciaDto> GetSolicitudByIdAsync(int solicitudId); // Método añadido

        Task<List<AgenciaDto>> GetAllAsync();
        Task<AgenciaDto?> GetByIdAsync(int id);
        Task<AgenciaDto?> GetByUsuarioIdAsync(int usuarioId);
        Task CrearAsync(CrearAgenciaDto nuevaAgencia);
        Task ActualizarAsync(UpdateAgenciaDto agenciaActualizada);
        Task EliminarAsync(int id);

        // Gestión de acompañantes
        Task<List<AcompananteDto>> GetAcompanantesByAgenciaIdAsync(int agenciaId);
        Task AgregarAcompananteAsync(int agenciaId, int acompananteId);
        Task RemoverAcompananteAsync(int agenciaId, int acompananteId);

        // Verificación de acompañantes
        Task<VerificacionDto> VerificarAcompananteAsync(int agenciaId, int acompananteId, VerificarAcompananteDto datosVerificacion);
        Task<List<AcompananteDto>> GetAcompanantesVerificadosAsync(int agenciaId);
        Task<List<AcompananteDto>> GetAcompanantesPendientesVerificacionAsync(int agenciaId);

        // Gestión de anuncios
        Task<AnuncioDestacadoDto> CrearAnuncioDestacadoAsync(CrearAnuncioDestacadoDto anuncioDto);
        Task<List<AnuncioDestacadoDto>> GetAnunciosByAgenciaAsync(int agenciaId);

        // Estadísticas y métricas
        Task<AgenciaEstadisticasDto> GetEstadisticasAgenciaAsync(int agenciaId);

        // Comisiones y beneficios
        Task<ComisionesDto> GetComisionesByAgenciaAsync(int agenciaId, DateTime fechaInicio, DateTime fechaFin);

        // Solo para administradores
        Task<bool> VerificarAgenciaAsync(int agenciaId, bool verificada);
        Task<List<AgenciaPendienteVerificacionDto>> GetAgenciasPendientesVerificacionAsync();

        Task<List<AgenciaDisponibleDto>> GetAgenciasDisponiblesAsync();

        Task<List<SolicitudAgenciaDto>> GetSolicitudesPendientesAsync();
        Task AprobarSolicitudAsync(int solicitudId);
        Task RechazarSolicitudAsync(int solicitudId);

        Task EnviarSolicitudAsync(int agenciaId);
        Task<PerfilEstadisticasDto?> GetEstadisticasPerfilAsync(int acompananteId);

        //nuevos Agregados

        Task<AgenciaDashboardDto> GetDashboardAsync(int agenciaId);
        Task<int> GetAgenciaIdByUsuarioIdAsync(int usuarioId);

        Task<AcompanantesIndependientesResponseDto> GetAcompanantesIndependientesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string filterBy = null,
            string sortBy = "Id",
            bool sortDesc = false);

        Task<SolicitudesHistorialResponseDto> GetHistorialSolicitudesAsync(
            int agenciaId,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string estado = null,
            int pageNumber = 1,
            int pageSize = 10);


        Task CancelarSolicitudAsync(int solicitudId, int usuarioId, string motivo);


        Task<agencia> CrearPendienteAsync(CrearSolicitudRegistroAgenciaDto dto);


        Task<int> SolicitarRegistroAgenciaAsync(CrearSolicitudRegistroAgenciaDto dto);
        Task<List<SolicitudRegistroAgenciaDto>> GetSolicitudesRegistroPendientesAsync();
        Task<bool> AprobarSolicitudRegistroAgenciaAsync(int solicitudId);
        Task<bool> RechazarSolicitudRegistroAgenciaAsync(int solicitudId, string motivo);


        Task<PuntosAgenciaDto> GetPuntosAgenciaAsync(int agenciaId);
        Task<int> OtorgarPuntosAgenciaAsync(OtorgarPuntosAgenciaDto dto);
        Task<bool> GastarPuntosAgenciaAsync(int agenciaId, int puntos, string concepto);

        Task<List<VerificacionDto>> VerificarAcompanantesLoteAsync(VerificacionLoteDto dto);

         Task CompletarPerfilAgenciaAsync(CompletarPerfilAgenciaDto dto);

        Task<bool> InvitarAcompananteAsync(int acompananteId);

        Task<int> GetAgenciaIdByAcompananteIdAsync(int acompananteId);


        Task<AnuncioDestacadoDto> GetAnuncioByReferenceIdAsync(string referenceId);
        Task UpdateAnuncioAsync(AnuncioDestacadoDto anuncio);
        Task<bool> ConfirmarPagoVerificacionAsync(int pagoId, string referenciaPago);
        Task<bool> EsPropietarioAgenciaAsync(int agenciaId, int usuarioId);


    }
}
