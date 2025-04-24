using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.Interfaces.Services.Foto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FotosController : ControllerBase
    {
        private readonly IFotoService _fotoService;
        private readonly ILogger<FotosController> _logger;

        public FotosController(
            IFotoService fotoService,
            ILogger<FotosController> logger)
        {
            _fotoService = fotoService;
            _logger = logger;
        }

        [HttpGet("acompanante/{acompananteId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<FotoDto>>> GetByAcompananteId(int acompananteId)
        {
            try
            {
                var fotos = await _fotoService.GetByAcompananteIdAsync(acompananteId);
                return Ok(fotos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fotos del acompañante {AcompananteId}", acompananteId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FotoDto>> GetById(int id)
        {
            try
            {
                var foto = await _fotoService.GetByIdAsync(id);
                if (foto == null)
                    return NotFound();

                return Ok(foto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener foto con ID {FotoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<FotoDto>> SubirFoto([FromForm] SubirFotoDto dto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var foto = await _fotoService.SubirFotoAsync(dto, usuarioId);
                return CreatedAtAction(nameof(GetById), new { id = foto.Id }, foto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al subir foto para acompañante {AcompananteId}", dto.AcompananteId);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al subir foto para acompañante {AcompananteId}", dto.AcompananteId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto para acompañante {AcompananteId}", dto.AcompananteId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FotoDto>> ActualizarFoto(int id, [FromBody] ActualizarFotoDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest("El ID de la ruta no coincide con el ID del objeto");

                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var foto = await _fotoService.ActualizarFotoAsync(dto, usuarioId);
                return Ok(foto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al actualizar foto con ID {FotoId}", id);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al actualizar foto con ID {FotoId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar foto con ID {FotoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarFoto(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var resultado = await _fotoService.EliminarFotoAsync(id, usuarioId);
                if (!resultado)
                    return NotFound();

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar foto con ID {FotoId}", id);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al eliminar foto con ID {FotoId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar foto con ID {FotoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

        [HttpPost("{fotoId}/principal/{acompananteId}")]
        public async Task<ActionResult<FotoDto>> EstablecerFotoPrincipal(int fotoId, int acompananteId)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var foto = await _fotoService.EstablecerFotoPrincipalAsync(fotoId, acompananteId, usuarioId);
                return Ok(foto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al establecer foto principal {FotoId} para acompañante {AcompananteId}", fotoId, acompananteId);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al establecer foto principal {FotoId} para acompañante {AcompananteId}", fotoId, acompananteId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer foto principal {FotoId} para acompañante {AcompananteId}", fotoId, acompananteId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

    }
}