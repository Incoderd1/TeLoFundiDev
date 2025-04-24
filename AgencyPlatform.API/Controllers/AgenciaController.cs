using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Application.DTOs.Agencias.AgenciaDah;
using AgencyPlatform.Application.DTOs.Anuncios;
using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Application.DTOs.Solicitudes;
using AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia;
using AgencyPlatform.Application.DTOs.Verificacion;
using AgencyPlatform.Application.DTOs.Verificaciones;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using AgencyPlatform.Application.Interfaces.Services.PagoVerificacion;
using AgencyPlatform.Shared.Exceptions;
using AgencyPlatform.Shared.Exporting.Models;
using AgencyPlatform.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AgenciaController : ControllerBase
    {
        private readonly IAgenciaService _agenciaService;
        private readonly IEmailSender _emailSender; // Inyectamos IEmailSender
        private readonly IUserService _userService; // Inyectamos IUserService
        private readonly IPagoVerificacionService _pagoVerificacionService;



        public AgenciaController(IAgenciaService agenciaService, IEmailSender emailSender, IUserService userService, IPagoVerificacionService pagoVerificacionService)
        {
            _agenciaService = agenciaService;
            _emailSender = emailSender;
            _userService = userService;
            _pagoVerificacionService = pagoVerificacionService;
        }

        // 🔹 CRUD
        [HttpGet]
        public async Task<ActionResult<List<AgenciaDto>>> GetAll()
            => Ok(await _agenciaService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<AgenciaDto>> GetById(int id)
        {
            var result = await _agenciaService.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("usuario")]
        public async Task<ActionResult<AgenciaDto>> GetByUsuarioActual()
            => Ok(await _agenciaService.GetByUsuarioIdAsync(GetUsuarioId()));


        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] UpdateAgenciaDto dto)
        {
            await _agenciaService.ActualizarAsync(dto);
            return Ok(new { mensaje = "Agencia actualizada correctamente" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _agenciaService.EliminarAsync(id);
            return Ok(new { mensaje = "Agencia eliminada correctamente" });
        }

        // 🔹 Acompañantes
        [HttpGet("{agenciaId}/acompanantes")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AcompananteDto>>> GetAcompanantes(int agenciaId)
            => Ok(await _agenciaService.GetAcompanantesByAgenciaIdAsync(agenciaId));

        [HttpPost("{agenciaId}/acompanantes/{acompananteId}")]
        [Authorize(Policy = "AgenciaOwnerOnly")]

        public async Task<IActionResult> AgregarAcompanante(int agenciaId, int acompananteId)
        {
            await _agenciaService.AgregarAcompananteAsync(agenciaId, acompananteId);
            return Ok("Acompañante asignado correctamente");
        }

        [HttpDelete("{agenciaId}/acompanantes/{acompananteId}")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<IActionResult> RemoverAcompanante(int agenciaId, int acompananteId)
        {
            await _agenciaService.RemoverAcompananteAsync(agenciaId, acompananteId);
            return Ok("Acompañante removido correctamente");
        }

        // 🔹 Verificaciones
        [HttpPost("{agenciaId}/verificar/{acompananteId}")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<VerificacionDto>> VerificarAcompanante(int agenciaId, int acompananteId, [FromBody] VerificarAcompananteDto dto)
            => Ok(await _agenciaService.VerificarAcompananteAsync(agenciaId, acompananteId, dto));

        [HttpGet("{agenciaId}/verificados")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AcompananteDto>>> GetVerificados(int agenciaId)
            => Ok(await _agenciaService.GetAcompanantesVerificadosAsync(agenciaId));

        [HttpGet("{agenciaId}/pendientes-verificacion")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AcompananteDto>>> GetPendientesVerificacion(int agenciaId)
            => Ok(await _agenciaService.GetAcompanantesPendientesVerificacionAsync(agenciaId));

        // 🔹 Anuncios destacados
        [HttpPost("anuncios")]
        public async Task<ActionResult<AnuncioDestacadoDto>> CrearAnuncio([FromBody] CrearAnuncioDestacadoDto dto)
            => Ok(await _agenciaService.CrearAnuncioDestacadoAsync(dto));

        [HttpGet("{agenciaId}/anuncios")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AnuncioDestacadoDto>>> GetAnuncios(int agenciaId)
            => Ok(await _agenciaService.GetAnunciosByAgenciaAsync(agenciaId));

        // 🔹 Estadísticas
        [HttpGet("{agenciaId}/estadisticas")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<AgenciaEstadisticasDto>> GetEstadisticas(int agenciaId)
            => Ok(await _agenciaService.GetEstadisticasAgenciaAsync(agenciaId));

        [HttpGet("{agenciaId}/comisiones")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<ComisionesDto>> GetComisiones(
             int agenciaId,
             [FromQuery] DateTime fechaInicio,
             [FromQuery] DateTime fechaFin)
        {
            var desdeUtc = fechaInicio.ToUniversalTime();
            var hastaUtc = fechaFin.ToUniversalTime();

            return Ok(await _agenciaService.GetComisionesByAgenciaAsync(agenciaId, desdeUtc, hastaUtc));
        }

        // 🔹 Admin
        [HttpPut("{agenciaId}/verificar")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> VerificarAgencia(int agenciaId, [FromQuery] bool verificada)
        {
            var result = await _agenciaService.VerificarAgenciaAsync(agenciaId, verificada);
            return Ok(new { mensaje = result ? "Agencia verificada" : "Agencia desverificada" });
        }

        [HttpGet("pendientes-verificacion")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<AgenciaPendienteVerificacionDto>>> GetPendientes()
            => Ok(await _agenciaService.GetAgenciasPendientesVerificacionAsync());


        [HttpGet("solicitudes")]
        //[Authorize(Roles = "agencia")]
        public async Task<IActionResult> GetSolicitudesPendientes()
        {
            var solicitudes = await _agenciaService.GetSolicitudesPendientesAsync();
            return Ok(solicitudes);
        }

        [HttpPut("solicitudes/{solicitudId}/aprobar")]
        public async Task<IActionResult> AprobarSolicitud(int solicitudId)
        {
            await _agenciaService.AprobarSolicitudAsync(solicitudId);
            return Ok(new { mensaje = "Solicitud aprobada y acompañante asignado a la agencia." });
        }

        [HttpPost("solicitudes/{id}/rechazar")]
        //[Authorize(Roles = "agencia")]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            await _agenciaService.RechazarSolicitudAsync(id);
            return Ok(new { mensaje = "Solicitud rechazada correctamente" });
        }

        [HttpGet("estadisticas/perfil/{acompananteId}")]
        public async Task<IActionResult> GetEstadisticasPorPerfil(int acompananteId)
        {
            var estadisticas = await _agenciaService.GetEstadisticasPerfilAsync(acompananteId);
            return Ok(estadisticas);
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AgenciaDashboardDto>> GetDashboard()
        {
            try
            {
                int usuarioId = GetUsuarioId();
                int agenciaId = await _agenciaService.GetAgenciaIdByUsuarioIdAsync(usuarioId);

                if (agenciaId <= 0)
                    return NotFound(new { mensaje = "No se encontró una agencia asociada a este usuario" });

                var dashboard = await _agenciaService.GetDashboardAsync(agenciaId);
                return Ok(dashboard);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
        [HttpGet("independientes")]
        public async Task<ActionResult<AcompanantesIndependientesResponseDto>> GetIndependientes(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string filterBy = null,
                [FromQuery] string sortBy = "Id",
                [FromQuery] bool sortDesc = false)
        {
            try
            {
                var resultado = await _agenciaService.GetAcompanantesIndependientesAsync(
                    pageNumber, pageSize, filterBy, sortBy, sortDesc);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
        [HttpGet("historial-solicitudes")]
        public async Task<ActionResult<SolicitudesHistorialResponseDto>> GetHistorialSolicitudes(
      [FromQuery] DateTime? fechaDesde = null,
      [FromQuery] DateTime? fechaHasta = null,
      [FromQuery] string estado = null,
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10)
        {
            try
            {
                int usuarioId = GetUsuarioId();
                int agenciaId = await _agenciaService.GetAgenciaIdByUsuarioIdAsync(usuarioId);

                if (agenciaId <= 0)
                    return NotFound(new { mensaje = "No se encontró una agencia asociada a este usuario" });

                var resultado = await _agenciaService.GetHistorialSolicitudesAsync(
                    agenciaId,
                    fechaDesde.ToUtcSafe(),
                    fechaHasta.ToUtcSafe(),
                    estado,
                    pageNumber,
                    pageSize);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        [HttpPut("solicitudes/{solicitudId}/cancelar")]
        public async Task<IActionResult> CancelarSolicitud(int solicitudId, [FromBody] CancelarSolicitudDto dto)
        {
            try
            {
                int usuarioId = GetUsuarioId();
                await _agenciaService.CancelarSolicitudAsync(solicitudId, usuarioId, dto.Motivo);
                return Ok(new { mensaje = "Solicitud cancelada correctamente" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Loguear el error
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
            }

        }
        [HttpGet("historial-solicitudes/exportar")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<IActionResult> ExportarHistorialSolicitudes(
             [FromQuery] DateTime? fechaDesde = null,
             [FromQuery] DateTime? fechaHasta = null,
             [FromQuery] string estado = null,
             [FromQuery] string formato = "csv")
        {
            try
            {
                int usuarioId = GetUsuarioId();
                int agenciaId = await _agenciaService.GetAgenciaIdByUsuarioIdAsync(usuarioId);

                var resultado = await _agenciaService.GetHistorialSolicitudesAsync(
                    agenciaId,
                    fechaDesde.ToUtcSafe(),
                    fechaHasta.ToUtcSafe(),
                    estado,
                    1,
                    int.MaxValue);

                if (!resultado.Items.Any())
                    return BadRequest(new { mensaje = "No hay solicitudes para exportar." });

                if (formato.ToLower() == "pdf")
                {
                    return BadRequest(new { mensaje = "Exportación PDF no implementada aún" });
                }

                var csv = CsvExporter.ExportToCsv(resultado.Items);
                return File(csv, "text/csv", $"historial_solicitudes_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al exportar", error = ex.Message });
            }
        }



        [HttpGet("historial-solicitudes/pdf")]
        public async Task<IActionResult> DescargarHistorialPdf(
              [FromQuery] DateTime? fechaDesde = null,
              [FromQuery] DateTime? fechaHasta = null,
              [FromQuery] string estado = null)
        {
            try
            {
                int usuarioId = GetUsuarioId();
                int agenciaId = await _agenciaService.GetAgenciaIdByUsuarioIdAsync(usuarioId);

                if (agenciaId <= 0)
                    return NotFound(new { mensaje = "No se encontró una agencia asociada a este usuario" });

                // ✅ Usar extensión para fechas UTC seguras
                var desdeUtc = fechaDesde.ToUtcSafe();
                var hastaUtc = fechaHasta.ToUtcSafe();

                var resultado = await _agenciaService.GetHistorialSolicitudesAsync(
                    agenciaId, desdeUtc, hastaUtc, estado, 1, int.MaxValue);

                if (resultado.Items == null || resultado.Items.Count == 0)
                    return BadRequest(new { mensaje = "No hay solicitudes para exportar." });

                var exportData = resultado.Items.Select(s => new SolicitudAgenciaExportDto
                {
                    Id = s.Id,
                    NombreAcompanante = s.NombreAcompanante,
                    NombreAgencia = s.NombreAgencia,
                    Estado = s.Estado,
                    FechaSolicitud = s.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"),
                    FechaRespuesta = s.FechaRespuesta?.ToString("dd/MM/yyyy HH:mm") ?? "-",
                    MotivoRechazo = s.MotivoRechazo,
                    MotivoCancelacion = s.MotivoCancelacion
                }).ToList();

                var pdfBytes = PdfExporter.ExportSolicitudes(exportData);

                return File(pdfBytes, "application/pdf", $"historial_solicitudes_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error generando el PDF", error = ex.Message });
            }
        }
        [HttpPost("solicitar-registro")]
        [AllowAnonymous] // Permite acceso sin autenticación
        public async Task<IActionResult> SolicitarRegistro([FromBody] CrearSolicitudRegistroAgenciaDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new { mensaje = "Los campos obligatorios deben ser completados." });
                }

                var solicitudId = await _agenciaService.SolicitarRegistroAgenciaAsync(dto);

                return Ok(new
                {
                    mensaje = "Tu solicitud ha sido enviada y está en proceso de revisión. Te notificaremos por email cuando haya sido procesada.",
                    solicitudId = solicitudId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Para listar solicitudes pendientes (solo admin)
        [HttpGet("solicitudes-registro/pendientes")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetSolicitudesRegistroPendientes()
        {
            try
            {
                var solicitudes = await _agenciaService.GetSolicitudesRegistroPendientesAsync();
                return Ok(solicitudes);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("solicitudes-registro/{solicitudId}/aprobar")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AprobarSolicitudRegistro(int solicitudId)
        {
            try
            {
                await _agenciaService.AprobarSolicitudRegistroAgenciaAsync(solicitudId);
                return Ok(new { mensaje = "Solicitud aprobada correctamente. Se ha creado la cuenta de agencia." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("solicitudes-registro/{solicitudId}/rechazar")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RechazarSolicitudRegistro(int solicitudId, [FromBody] RechazarSolicitudRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Motivo))
                {
                    return BadRequest(new { mensaje = "Se debe proporcionar un motivo para el rechazo." });
                }

                await _agenciaService.RechazarSolicitudRegistroAgenciaAsync(solicitudId, request.Motivo);
                return Ok(new { mensaje = "Solicitud rechazada correctamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
        [HttpGet("{agenciaId}/puntos")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<PuntosAgenciaDto>> GetPuntosAgencia(int agenciaId)
        {
            try
            {
                var puntos = await _agenciaService.GetPuntosAgenciaAsync(agenciaId);
                return Ok(puntos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("puntos/gastar")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<IActionResult> GastarPuntos([FromBody] GastarPuntosRequest request)
        {
            try
            {
                var agenciaId = await _agenciaService.GetAgenciaIdByUsuarioIdAsync(GetUsuarioId());
                var resultado = await _agenciaService.GastarPuntosAgenciaAsync(
                    agenciaId, request.Puntos, request.Concepto);

                return Ok(new { mensaje = "Puntos gastados correctamente", exito = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
        [HttpPost("verificar-lote")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<VerificacionDto>>> VerificarLote([FromBody] VerificacionLoteDto dto)
        {
            try
            {
                var resultados = await _agenciaService.VerificarAcompanantesLoteAsync(dto);
                return Ok(new
                {
                    mensaje = $"Se han verificado {resultados.Count} acompañantes exitosamente",
                    verificaciones = resultados
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
        // Agregar estos endpoints al controlador de agencias
        [HttpGet("{agenciaId}/pagos-verificacion")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<PagoVerificacionDto>>> GetPagosVerificacion(int agenciaId)
        {
            try
            {
                var pagos = await _pagoVerificacionService.GetPagosByAgenciaIdAsync(agenciaId);
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{agenciaId}/pagos-verificacion/pendientes")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<PagoVerificacionDto>>> GetPagosPendientes(int agenciaId)
        {
            try
            {
                var pagos = await _pagoVerificacionService.GetPagosPendientesByAgenciaIdAsync(agenciaId);
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("pagos-verificacion/{pagoId}/confirmar")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<IActionResult> ConfirmarPago(int pagoId, [FromBody] ConfirmarPagoDto dto)
        {
            try
            {
                await _pagoVerificacionService.ConfirmarPagoAsync(pagoId, dto.ReferenciaPago);
                return Ok(new { mensaje = "Pago confirmado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        public class ConfirmarPagoDto
        {
            public string ReferenciaPago { get; set; }
        }

        public class GastarPuntosRequest
        {
            public int Puntos { get; set; }
            public string Concepto { get; set; }
        }

        // 🔐 Utilidadk
        private int GetUsuarioId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : throw new UnauthorizedAccessException("Usuario no autenticado");
        }
    }
}
