using AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitudesAgenciaController : ControllerBase
    {
        private readonly IAgenciaService _agenciaService;

        public SolicitudesAgenciaController(IAgenciaService agenciaService)
        {
            _agenciaService = agenciaService;
        }

        [HttpPost("solicitar-registro")]
        [AllowAnonymous]
        public async Task<IActionResult> SolicitarRegistro([FromBody] CrearSolicitudRegistroAgenciaDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new { Message = "Los campos obligatorios deben ser completados." });
                }

                var solicitudId = await _agenciaService.SolicitarRegistroAgenciaAsync(dto);

                return Ok(new
                {
                    Message = "Tu solicitud ha sido enviada y está en proceso de revisión. Te notificaremos por email cuando haya sido procesada.",
                    SolicitudId = solicitudId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("pendientes")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPendientes()
        {
            try
            {
                var solicitudes = await _agenciaService.GetSolicitudesRegistroPendientesAsync();
                return Ok(solicitudes);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("aprobar/{solicitudId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Aprobar(int solicitudId)
        {
            try
            {
                await _agenciaService.AprobarSolicitudRegistroAgenciaAsync(solicitudId);
                return Ok(new { Message = "Solicitud aprobada correctamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("rechazar/{solicitudId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Rechazar(int solicitudId, [FromBody] RechazarSolicitudRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Motivo))
                {
                    return BadRequest(new { Message = "Se debe proporcionar un motivo para el rechazo." });
                }

                await _agenciaService.RechazarSolicitudRegistroAgenciaAsync(solicitudId, request.Motivo);
                return Ok(new { Message = "Solicitud rechazada correctamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
    public class RechazarSolicitudRequest
    {
        public string Motivo { get; set; }
    }
}

