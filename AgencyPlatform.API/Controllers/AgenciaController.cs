using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Application.DTOs.Agencias.AgenciaDah;
using AgencyPlatform.Application.DTOs.Anuncios;
using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Application.DTOs.Solicitudes;
using AgencyPlatform.Application.DTOs.SolicitudesAgencia;
using AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia;
using AgencyPlatform.Application.DTOs.Verificacion;
using AgencyPlatform.Application.DTOs.Verificaciones;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using AgencyPlatform.Application.Interfaces.Services.Notificaciones;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Core.Enums;
using AgencyPlatform.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AgencyPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class AgenciaController : ControllerBase
    {
        private readonly IAgenciaService _agenciaService;
        private readonly INotificacionService _notificacionService;
        private readonly ILogger<AgenciaController> _logger;

        public AgenciaController(
            IAgenciaService agenciaService,
            INotificacionService notificacionService,
            ILogger<AgenciaController> logger)
        {
            _agenciaService = agenciaService;
            _notificacionService = notificacionService;
            _logger = logger;
        }

        #region Operaciones CRUD básicas

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<AgenciaDto>>> GetAllAgencias()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las agencias por usuario ID: {UsuarioId}", GetUserId());
                var agencias = await _agenciaService.GetAllAsync();
                return Ok(agencias);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener todas las agencias");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las agencias");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgenciaDto>> GetAgenciaById(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo agencia con ID: {AgenciaId} por usuario ID: {UsuarioId}", id, GetUserId());
                var usuarioId = GetUserId();
                var esPropiaAgencia = await VerificarAgenciaPropiaAsync(id, usuarioId);
                var esAdmin = User.IsInRole("admin");

                if (!esPropiaAgencia && !esAdmin)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para ver la agencia ID: {AgenciaId}", usuarioId, id);
                    return Unauthorized(new { Message = "No tienes permisos para ver esta agencia." });
                }

                var agencia = await _agenciaService.GetByIdAsync(id);
                if (agencia == null)
                {
                    _logger.LogWarning("Agencia con ID: {AgenciaId} no encontrada", id);
                    return NotFound(new { Message = $"Agencia con ID {id} no encontrada." });
                }

                return Ok(agencia);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener agencia ID: {AgenciaId}", id);
                return Unauthorized(new { Message = "No tienes permisos para ver esta agencia." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agencia por ID");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("mi-agencia")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<AgenciaDto>> GetMiAgencia()
        {
            try
            {
                _logger.LogInformation("Obteniendo agencia del usuario actual ID: {UsuarioId}", GetUserId());
                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);

                if (agencia == null)
                {
                    _logger.LogWarning("No se encontró una agencia asociada al usuario ID: {UsuarioId}", usuarioId);
                    return NotFound(new { Message = "No tienes una agencia asociada." });
                }

                return Ok(agencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agencia del usuario actual");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<AgenciaDto>> CrearAgencia([FromBody] CrearAgenciaDto agenciaDto)
        {
            try
            {
                _logger.LogInformation("Creando nueva agencia por usuario ID: {UsuarioId}", GetUserId());
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _agenciaService.CrearAsync(agenciaDto);
                var usuarioId = GetUserId();
                var agenciaCreada = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agenciaCreada == null)
                {
                    return StatusCode(500, new { Message = "Error al obtener la agencia creada." });
                }

                return CreatedAtAction(nameof(GetAgenciaById), new { id = agenciaCreada.Id }, agenciaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear agencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult> ActualizarAgencia(int id, [FromBody] UpdateAgenciaDto agenciaDto)
        {
            try
            {
                _logger.LogInformation("Actualizando agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", id, GetUserId());
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != agenciaDto.Id)
                {
                    return BadRequest(new { Message = "El ID de la ruta no coincide con el ID del DTO." });
                }

                await _agenciaService.ActualizarAsync(agenciaDto);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al actualizar agencia ID: {AgenciaId}", id);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar agencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> EliminarAgencia(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", id, GetUserId());
                await _agenciaService.EliminarAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar agencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Registro de Agencias

        [HttpPost("solicitar-registro")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> SolicitarRegistro([FromBody] CrearSolicitudRegistroAgenciaDto dto)
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

        [HttpGet("solicitudes-registro/pendientes")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<SolicitudRegistroAgenciaDto>>> GetSolicitudesRegistroPendientes()
        {
            try
            {
                _logger.LogInformation("Obteniendo solicitudes de registro pendientes por usuario ID: {UsuarioId}", GetUserId());
                var solicitudes = await _agenciaService.GetSolicitudesRegistroPendientesAsync();
                return Ok(solicitudes);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener solicitudes de registro pendientes");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes de registro pendientes");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("solicitudes-registro/{solicitudId}/aprobar")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AprobarSolicitudRegistro(int solicitudId)
        {
            try
            {
                _logger.LogInformation("Aprobando solicitud de registro ID: {SolicitudId} por usuario ID: {UsuarioId}", solicitudId, GetUserId());
                await _agenciaService.AprobarSolicitudRegistroAgenciaAsync(solicitudId);
                return Ok(new { Message = "Solicitud aprobada exitosamente." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al aprobar solicitud de registro ID: {SolicitudId}", solicitudId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar solicitud de registro");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("solicitudes-registro/{solicitudId}/rechazar")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> RechazarSolicitudRegistro(int solicitudId, [FromBody] string motivo)
        {
            try
            {
                _logger.LogInformation("Rechazando solicitud de registro ID: {SolicitudId} por usuario ID: {UsuarioId}", solicitudId, GetUserId());
                await _agenciaService.RechazarSolicitudRegistroAgenciaAsync(solicitudId, motivo);
                return Ok(new { Message = "Solicitud rechazada exitosamente." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al rechazar solicitud de registro ID: {SolicitudId}", solicitudId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar solicitud de registro");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Gestión de Acompañantes

        [HttpGet("{agenciaId}/acompanantes")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AcompananteDto>>> GetAcompanantes(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes de agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var acompanantes = await _agenciaService.GetAcompanantesByAgenciaIdAsync(agenciaId);
                return Ok(acompanantes);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener acompañantes de agencia ID: {AgenciaId}", agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes de la agencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("{agenciaId}/acompanantes/{acompananteId}")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult> AgregarAcompanante(int agenciaId, int acompananteId)
        {
            try
            {
                _logger.LogInformation("Agregando acompañante ID: {AcompananteId} a agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", acompananteId, agenciaId, GetUserId());
                await _agenciaService.AgregarAcompananteAsync(agenciaId, acompananteId);
                return Ok(new { Message = "Acompañante agregado exitosamente." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al agregar acompañante ID: {AcompananteId} a agencia ID: {AgenciaId}", acompananteId, agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar acompañante");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpDelete("{agenciaId}/acompanantes/{acompananteId}")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult> RemoverAcompanante(int agenciaId, int acompananteId)
        {
            try
            {
                _logger.LogInformation("Eliminando acompañante ID: {AcompananteId} de agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", acompananteId, agenciaId, GetUserId());
                await _agenciaService.RemoverAcompananteAsync(agenciaId, acompananteId);
                return Ok(new { Message = "Acompañante removido exitosamente." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al remover acompañante ID: {AcompananteId} de agencia ID: {AgenciaId}", acompananteId, agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al remover acompañante");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("acompanantes-independientes")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<AcompanantesIndependientesResponseDto>> GetAcompanantesIndependientes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string filterBy = null,
            [FromQuery] string sortBy = "Id",
            [FromQuery] bool sortDesc = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes independientes por usuario ID: {UsuarioId}", GetUserId());
                var result = await _agenciaService.GetAcompanantesIndependientesAsync(
                    pageNumber, pageSize, filterBy, sortBy, sortDesc);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes independientes");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("invitar-acompanante/{acompananteId}")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult> InvitarAcompanante(int acompananteId)
        {
            try
            {
                _logger.LogInformation("Invitando acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", acompananteId, GetUserId());
                await _agenciaService.InvitarAcompananteAsync(acompananteId);
                return Ok(new { Message = "Invitación enviada exitosamente." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al invitar acompañante ID: {AcompananteId}", acompananteId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar invitación");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Verificación de Acompañantes

        [HttpPost("{agenciaId}/verificar/{acompananteId}")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<VerificacionDto>> VerificarAcompanante(
            int agenciaId,
            int acompananteId,
            [FromBody] VerificarAcompananteDto datosVerificacion)
        {
            try
            {
                _logger.LogInformation("Verificando acompañante ID: {AcompananteId} en agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", acompananteId, agenciaId, GetUserId());
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _agenciaService.VerificarAcompananteAsync(
                    agenciaId, acompananteId, datosVerificacion);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al verificar acompañante ID: {AcompananteId} en agencia ID: {AgenciaId}", acompananteId, agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (BusinessRuleViolationException ex)
            {
                _logger.LogWarning(ex, "Regla de negocio violada al verificar acompañante ID: {AcompananteId}", acompananteId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar acompañante");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("verificar-lote")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<List<VerificacionDto>>> VerificarAcompanantesLote([FromBody] VerificacionLoteDto dto)
        {
            try
            {
                _logger.LogInformation("Verificando acompañantes en lote para agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", dto.AgenciaId, GetUserId());
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null || agencia.Id != dto.AgenciaId)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para verificar acompañantes en la agencia ID: {AgenciaId}", usuarioId, dto.AgenciaId);
                    return Unauthorized(new { Message = "No tienes permisos para verificar acompañantes en esta agencia." });
                }

                var resultado = await _agenciaService.VerificarAcompanantesLoteAsync(dto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar acompañantes en lote");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("{agenciaId}/verificados")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AcompananteDto>>> GetAcompanantesVerificados(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes verificados de agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var acompanantes = await _agenciaService.GetAcompanantesVerificadosAsync(agenciaId);
                return Ok(acompanantes);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener acompañantes verificados de agencia ID: {AgenciaId}", agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes verificados");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("{agenciaId}/pendientes-verificacion")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AcompananteDto>>> GetAcompanantesPendientesVerificacion(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes pendientes de verificación de agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var acompanantes = await _agenciaService.GetAcompanantesPendientesVerificacionAsync(agenciaId);
                return Ok(acompanantes);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener acompañantes pendientes de verificación de agencia ID: {AgenciaId}", agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes pendientes de verificación");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("pagos-verificacion/{pagoId}/confirmar")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult> ConfirmarPagoVerificacion(int pagoId, [FromBody] string referenciaPago)
        {
            try
            {
                _logger.LogInformation("Confirmando pago de verificación ID: {PagoId} por usuario ID: {UsuarioId}", pagoId, GetUserId());
                var resultado = await _agenciaService.ConfirmarPagoVerificacionAsync(pagoId, referenciaPago);
                if (resultado)
                {
                    return Ok(new { Message = "Pago confirmado exitosamente." });
                }
                else
                {
                    return BadRequest(new { Message = "No se pudo confirmar el pago." });
                }
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Pago de verificación ID: {PagoId} no encontrado", pagoId);
                return NotFound(new { Message = ex.Message });
            }
            catch (BusinessRuleViolationException ex)
            {
                _logger.LogWarning(ex, "Regla de negocio violada al confirmar pago ID: {PagoId}", pagoId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar pago de verificación");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Anuncios Destacados

        [HttpPost("anuncios")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<AnuncioDestacadoDto>> CrearAnuncioDestacado([FromBody] CrearAnuncioDestacadoDto dto)
        {
            try
            {
                _logger.LogInformation("Creando anuncio destacado por usuario ID: {UsuarioId}", GetUserId());
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene una agencia asociada", usuarioId);
                    return Unauthorized(new { Message = "No tienes una agencia asociada." });
                }

                // Validar que el acompañante pertenece a la agencia del usuario
                var agenciaIdDelAcompanante = await _agenciaService.GetAgenciaIdByAcompananteIdAsync(dto.AcompananteId);
                if (agenciaIdDelAcompanante != agencia.Id)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para crear un anuncio para el acompañante ID: {AcompananteId}", usuarioId, dto.AcompananteId);
                    return Unauthorized(new { Message = "No tienes permisos para crear un anuncio para este acompañante." });
                }

                var anuncio = await _agenciaService.CrearAnuncioDestacadoAsync(dto);
                return CreatedAtAction(nameof(GetAnuncioByReferenceId), new { referenceId = anuncio.Id }, anuncio);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al crear anuncio destacado");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (BusinessRuleViolationException ex)
            {
                _logger.LogWarning(ex, "Regla de negocio violada al crear anuncio destacado");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear anuncio destacado");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("anuncios/reference/{referenceId}")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<AnuncioDestacadoDto>> GetAnuncioByReferenceId(string referenceId)
        {
            try
            {
                _logger.LogInformation("Obteniendo anuncio por referencia ID: {ReferenceId} por usuario ID: {UsuarioId}", referenceId, GetUserId());
                var anuncio = await _agenciaService.GetAnuncioByReferenceIdAsync(referenceId);
                if (anuncio == null)
                {
                    _logger.LogWarning("Anuncio con referencia {ReferenceId} no encontrado", referenceId);
                    return NotFound(new { Message = $"Anuncio con referencia {referenceId} no encontrado." });
                }

                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene una agencia asociada", usuarioId);
                    return Unauthorized(new { Message = "No tienes una agencia asociada." });
                }

                var agenciaIdDelAcompanante = await _agenciaService.GetAgenciaIdByAcompananteIdAsync(anuncio.AcompananteId);
                if (agenciaIdDelAcompanante != agencia.Id)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para ver el anuncio con referencia {ReferenceId}", usuarioId, referenceId);
                    return Unauthorized(new { Message = "No tienes permisos para ver este anuncio." });
                }

                return Ok(anuncio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener anuncio por referencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("{agenciaId}/anuncios")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<List<AnuncioDestacadoDto>>> GetAnuncios(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo anuncios de agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var anuncios = await _agenciaService.GetAnunciosByAgenciaAsync(agenciaId);
                return Ok(anuncios);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener anuncios de agencia ID: {AgenciaId}", agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener anuncios");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPut("anuncios")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult> UpdateAnuncio([FromBody] AnuncioDestacadoDto anuncio)
        {
            try
            {
                _logger.LogInformation("Actualizando anuncio ID: {AnuncioId} por usuario ID: {UsuarioId}", anuncio.Id, GetUserId());
                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene una agencia asociada", usuarioId);
                    return Unauthorized(new { Message = "No tienes una agencia asociada." });
                }

                var agenciaIdDelAcompanante = await _agenciaService.GetAgenciaIdByAcompananteIdAsync(anuncio.AcompananteId);
                if (agenciaIdDelAcompanante != agencia.Id)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para actualizar el anuncio ID: {AnuncioId}", usuarioId, anuncio.Id);
                    return Unauthorized(new { Message = "No tienes permisos para actualizar este anuncio." });
                }

                await _agenciaService.UpdateAnuncioAsync(anuncio);
                return Ok(new { Message = "Anuncio actualizado exitosamente." });
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Anuncio ID: {AnuncioId} no encontrado", anuncio.Id);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar anuncio");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Estadísticas y Dashboard

        [HttpGet("{agenciaId}/dashboard")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<AgenciaDashboardDto>> GetDashboard(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo dashboard para agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var dashboard = await _agenciaService.GetDashboardAsync(agenciaId);
                return Ok(dashboard);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Agencia ID: {AgenciaId} no encontrada para dashboard", agenciaId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard de agencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("{agenciaId}/estadisticas")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<AgenciaEstadisticasDto>> GetEstadisticas(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas para agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var estadisticas = await _agenciaService.GetEstadisticasAgenciaAsync(agenciaId);
                return Ok(estadisticas);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener estadísticas de agencia ID: {AgenciaId}", agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("{agenciaId}/comisiones")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<ComisionesDto>> GetComisiones(
            int agenciaId,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("Obteniendo comisiones para agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var comisiones = await _agenciaService.GetComisionesByAgenciaAsync(agenciaId, fechaInicio, fechaFin);
                return Ok(comisiones);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener comisiones de agencia ID: {AgenciaId}", agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comisiones");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("estadisticas-perfil/{acompananteId}")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<PerfilEstadisticasDto>> GetEstadisticasPerfil(int acompananteId)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de perfil para acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", acompananteId, GetUserId());
                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene una agencia asociada", usuarioId);
                    return Unauthorized(new { Message = "No tienes una agencia asociada." });
                }

                var agenciaIdDelAcompanante = await _agenciaService.GetAgenciaIdByAcompananteIdAsync(acompananteId);
                if (agenciaIdDelAcompanante != agencia.Id)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para ver estadísticas del acompañante ID: {AcompananteId}", usuarioId, acompananteId);
                    return Unauthorized(new { Message = "No tienes permisos para ver las estadísticas de este acompañante." });
                }

                var estadisticas = await _agenciaService.GetEstadisticasPerfilAsync(acompananteId);
                if (estadisticas == null)
                {
                    _logger.LogWarning("Estadísticas no encontradas para acompañante ID: {AcompananteId}", acompananteId);
                    return NotFound(new { Message = "No se encontraron estadísticas para este perfil." });
                }

                return Ok(estadisticas);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener estadísticas de perfil para acompañante ID: {AcompananteId}", acompananteId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del perfil");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Solicitudes

        [HttpGet("solicitudes")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult<List<SolicitudAgenciaDto>>> GetSolicitudesPendientes()
        {
            try
            {
                _logger.LogInformation("Obteniendo solicitudes pendientes por usuario ID: {UsuarioId}", GetUserId());
                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene una agencia asociada", usuarioId);
                    return Unauthorized(new { Message = "No tienes una agencia asociada." });
                }

                var solicitudes = await _agenciaService.GetSolicitudesPendientesAsync();
                return Ok(solicitudes);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener solicitudes pendientes");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes pendientes");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPut("solicitudes/{solicitudId}/aprobar")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult> AprobarSolicitud(int solicitudId)
        {
            try
            {
                _logger.LogInformation("Aprobando solicitud ID: {SolicitudId} por usuario ID: {UsuarioId}", solicitudId, GetUserId());
                var usuarioId = GetUserId();
                var solicitud = await _agenciaService.GetSolicitudByIdAsync(solicitudId);
                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud ID: {SolicitudId} no encontrada", solicitudId);
                    return NotFound(new { Message = "Solicitud no encontrada" });
                }

                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null || solicitud.AgenciaId != agencia.Id)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para aprobar la solicitud ID: {SolicitudId} para la agencia {AgenciaId}", usuarioId, solicitudId, solicitud.AgenciaId);
                    return Unauthorized(new { Message = "No tienes permisos para aprobar esta solicitud." });
                }

                await _agenciaService.AprobarSolicitudAsync(solicitudId);
                return Ok(new { Message = "Solicitud aprobada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar solicitud");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPut("solicitudes/{solicitudId}/rechazar")]
        [Authorize(Roles = "agencia")]
        public async Task<IActionResult> RechazarSolicitud(int solicitudId)
        {
            try
            {
                _logger.LogInformation("Rechazando solicitud ID: {SolicitudId} por usuario ID: {UsuarioId}", solicitudId, GetUserId());
                var usuarioId = GetUserId();
                var solicitud = await _agenciaService.GetSolicitudByIdAsync(solicitudId);
                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud ID: {SolicitudId} no encontrada", solicitudId);
                    return NotFound(new { Message = "Solicitud no encontrada" });
                }

                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                if (agencia == null || solicitud.AgenciaId != agencia.Id)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para rechazar la solicitud ID: {SolicitudId} para la agencia {AgenciaId}", usuarioId, solicitudId, solicitud.AgenciaId);
                    return Unauthorized(new { Message = "No tienes permisos para rechazar esta solicitud." });
                }

                await _agenciaService.RechazarSolicitudAsync(solicitudId);
                return Ok(new { Message = "Solicitud rechazada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar solicitud");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("solicitudes/enviar/{agenciaId}")]
        [Authorize(Roles = "acompanante")]
        public async Task<ActionResult> EnviarSolicitud(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Enviando solicitud a agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                await _agenciaService.EnviarSolicitudAsync(agenciaId);
                return Ok(new { Message = "Solicitud enviada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar solicitud");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("historial-solicitudes")]
        [Authorize(Roles = "agencia,admin")]
        public async Task<ActionResult<SolicitudesHistorialResponseDto>> GetHistorialSolicitudes(
            [FromQuery] int? agenciaId = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string estado = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Obteniendo historial de solicitudes por usuario ID: {UsuarioId}", GetUserId());
                var usuarioId = GetUserId();
                if (!User.IsInRole("admin"))
                {
                    agenciaId = await _agenciaService.GetAgenciaIdByUsuarioIdAsync(usuarioId);
                    if (agenciaId == 0)
                    {
                        _logger.LogWarning("Usuario {UsuarioId} no tiene una agencia asociada para ver historial de solicitudes", usuarioId);
                        return Unauthorized(new { Message = "No tienes una agencia asociada para ver el historial de solicitudes." });
                    }
                }

                var historial = await _agenciaService.GetHistorialSolicitudesAsync(
                    agenciaId.Value,
                    fechaDesde,
                    fechaHasta,
                    estado,
                    pageNumber,
                    pageSize
                );

                return Ok(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de solicitudes");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPut("solicitudes/{solicitudId}/cancelar")]
        [Authorize]
        public async Task<ActionResult> CancelarSolicitud(int solicitudId, [FromBody] CancelarSolicitudDto dto)
        {
            try
            {
                _logger.LogInformation("Cancelando solicitud ID: {SolicitudId} por usuario ID: {UsuarioId}", solicitudId, GetUserId());
                var usuarioId = GetUserId();
                await _agenciaService.CancelarSolicitudAsync(solicitudId, usuarioId, dto.Motivo);
                return Ok(new { Message = "Solicitud cancelada exitosamente." });
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Solicitud ID: {SolicitudId} no encontrada", solicitudId);
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al cancelar solicitud ID: {SolicitudId}", solicitudId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al cancelar solicitud ID: {SolicitudId}", solicitudId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar solicitud");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("solicitudes/{solicitudId}")]
        [Authorize]
        public async Task<ActionResult<SolicitudAgenciaDto>> GetSolicitudById(int solicitudId)
        {
            try
            {
                _logger.LogInformation("Obteniendo solicitud con ID: {SolicitudId} por usuario ID: {UsuarioId}", solicitudId, GetUserId());
                var solicitud = await _agenciaService.GetSolicitudByIdAsync(solicitudId);
                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud con ID: {SolicitudId} no encontrada", solicitudId);
                    return NotFound(new { Message = $"Solicitud con ID {solicitudId} no encontrada." });
                }

                var usuarioId = GetUserId();
                var esAdmin = User.IsInRole("admin");
                var esAgencia = User.IsInRole("agencia");
                var esAcompanante = User.IsInRole("acompanante");

                if (esAgencia)
                {
                    var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
                    if (agencia == null || solicitud.AgenciaId != agencia.Id)
                    {
                        _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para ver la solicitud ID: {SolicitudId}", usuarioId, solicitudId);
                        return Unauthorized(new { Message = "No tienes permisos para ver esta solicitud." });
                    }
                }
                else if (esAcompanante)
                {
                    if (solicitud.AcompananteId != usuarioId)
                    {
                        _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para ver la solicitud ID: {SolicitudId}", usuarioId, solicitudId);
                        return Unauthorized(new { Message = "No tienes permisos para ver esta solicitud." });
                    }
                }
                else if (!esAdmin)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para ver la solicitud ID: {SolicitudId}", usuarioId, solicitudId);
                    return Unauthorized(new { Message = "No tienes permisos para ver esta solicitud." });
                }

                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitud por ID");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Sistema de Puntos

        [HttpGet("{agenciaId}/puntos")]
        [Authorize(Policy = "AgenciaOwnerOnly")]
        public async Task<ActionResult<PuntosAgenciaDto>> GetPuntos(int agenciaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo puntos de agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                var puntos = await _agenciaService.GetPuntosAgenciaAsync(agenciaId);
                return Ok(puntos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener puntos de agencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("puntos/gastar")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult> GastarPuntos([FromBody] GastarPuntosDto dto)
        {
            try
            {
                _logger.LogInformation("Gastando puntos por usuario ID: {UsuarioId}", GetUserId());
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuarioId = GetUserId();
                var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);

                if (agencia == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene una agencia asociada", usuarioId);
                    return NotFound(new { Message = "Agencia no encontrada." });
                }

                var resultado = await _agenciaService.GastarPuntosAgenciaAsync(agencia.Id, dto.Puntos, dto.Concepto);
                if (resultado)
                {
                    return Ok(new { Message = "Puntos gastados exitosamente." });
                }
                else
                {
                    return BadRequest(new { Message = "No se pudieron gastar los puntos." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al gastar puntos");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("{agenciaId}/puntos/otorgar")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> OtorgarPuntos(int agenciaId, [FromBody] OtorgarPuntosAgenciaDto dto)
        {
            try
            {
                _logger.LogInformation("Otorgando puntos a agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                dto.AgenciaId = agenciaId;
                var nuevoSaldo = await _agenciaService.OtorgarPuntosAgenciaAsync(dto);
                return Ok(new { Message = "Puntos otorgados exitosamente.", NuevoSaldo = nuevoSaldo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al otorgar puntos a agencia ID: {AgenciaId}", agenciaId);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        #endregion

        #region Administración

        [HttpGet("disponibles")]
        public async Task<ActionResult<List<AgenciaDisponibleDto>>> GetAgenciasDisponibles()
        {
            try
            {
                _logger.LogInformation("Obteniendo agencias disponibles");
                var agencias = await _agenciaService.GetAgenciasDisponiblesAsync();
                return Ok(agencias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agencias disponibles");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPut("{agenciaId}/verificar")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> VerificarAgencia(int agenciaId, [FromBody] bool verificada)
        {
            try
            {
                _logger.LogInformation("Verificando agencia ID: {AgenciaId} por usuario ID: {UsuarioId}", agenciaId, GetUserId());
                await _agenciaService.VerificarAgenciaAsync(agenciaId, verificada);
                var mensaje = verificada ? "Agencia verificada exitosamente." : "Verificación de agencia removida.";
                return Ok(new { Message = mensaje });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al verificar agencia ID: {AgenciaId}", agenciaId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar agencia");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpGet("pendientes-verificacion")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<AgenciaPendienteVerificacionDto>>> GetAgenciasPendientesVerificacion()
        {
            try
            {
                _logger.LogInformation("Obteniendo agencias pendientes de verificación por usuario ID: {UsuarioId}", GetUserId());
                var agencias = await _agenciaService.GetAgenciasPendientesVerificacionAsync();
                return Ok(agencias);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener agencias pendientes de verificación");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agencias pendientes de verificación");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        [HttpPost("completar-perfil")]
        [Authorize(Roles = "agencia")]
        public async Task<ActionResult> CompletarPerfil([FromBody] CompletarPerfilAgenciaDto dto)
        {
            try
            {
                _logger.LogInformation("Completando perfil de agencia por usuario ID: {UsuarioId}", GetUserId());
                await _agenciaService.CompletarPerfilAgenciaAsync(dto);
                return Ok(new { Message = "Perfil completado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar perfil de agencia");
               
                    return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Métodos auxiliares

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario.");
            }
            return userId;
        }

        private async Task<bool> VerificarAgenciaPropiaAsync(int agenciaId, int usuarioId)
        {
            var agencia = await _agenciaService.GetByUsuarioIdAsync(usuarioId);
            return agencia != null && agencia.Id == agenciaId;
        }
        public class GastarPuntosDto
        {
            [Required(ErrorMessage = "La cantidad de puntos es requerida")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad de puntos debe ser mayor a 0")]
            public int Puntos { get; set; }

            [Required(ErrorMessage = "El concepto es requerido")]
            [StringLength(255, ErrorMessage = "El concepto no puede exceder 255 caracteres")]
            public string Concepto { get; set; }
        }

        #endregion
    }
}