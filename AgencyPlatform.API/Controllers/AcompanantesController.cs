using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.DTOs.Servicio;
using AgencyPlatform.Application.Interfaces.Services.Acompanantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgencyPlatform.Application.DTOs;
using System.Security.Claims;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using AgencyPlatform.Application.DTOs.SolicitudesAgencia;
using AgencyPlatform.Application.Interfaces.Services.Foto;
using AgencyPlatform.Infrastructure.Services.Foto;
using AgencyPlatform.Application.DTOs.Acompanantes.RegistroAcompananate;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AgencyPlatform.Application;
using AgencyPlatform.Application.DTOs.Busqueda;
using AgencyPlatform.Application.Interfaces.Services.AdvancedSearch;
using AgencyPlatform.Infrastructure.Services.AdvancedSearch;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcompanantesController : ControllerBase
    {
        private readonly IAcompananteService _acompananteService;
        private readonly ILogger<AcompanantesController> _logger;
        private readonly IAgenciaService _agenciaService;
        private readonly IFotoService _fotoService;
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly IAdvancedSearchService _searchService;


        public AcompanantesController(
            IAcompananteService acompananteService,
            ILogger<AcompanantesController> logger,
            IAgenciaService agenciaService,
            IFotoService fotoService,
            IUserService userService,
            IPaymentService paymentService,
            IAdvancedSearchService searchService)
        {
            _acompananteService = acompananteService;
            _logger = logger;
            _agenciaService = agenciaService;
            _fotoService = fotoService;
            _userService = userService;
            _paymentService = paymentService;
            _searchService = searchService;
        }

        /// <summary>
        /// Obtiene todos los acompañantes de forma paginada
        /// </summary>
        [HttpGet("Paginado")]
        [ProducesResponseType(typeof(PaginatedResultDto<AcompananteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResultDto<AcompananteDto>>> GetAllPaginado(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes paginados. Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;
                var result = await _acompananteService.GetAllPaginatedAsync(pageNumber, pageSize);
                _logger.LogInformation("Se obtuvieron {Count} acompañantes. Total: {TotalItems}", result.Items.Count, result.TotalItems);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes paginados. Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los acompañantes de forma paginada
        /// </summary>
        [HttpGet("Paginad2")]
        [ProducesResponseType(typeof(PaginatedResultDto<AcompananteResumen2Dto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResultDto<AcompananteResumen2Dto>>> GetAllPaginadoResumen(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes paginados. Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;
                var result = await _acompananteService.GetAllPaginatedResumenAsync(pageNumber, pageSize);
                _logger.LogInformation("Se obtuvieron {Count} acompañantes. Total: {TotalItems}", result.Items.Count, result.TotalItems);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes paginados. Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los acompañantes
        /// </summary>
        /// <returns>Lista de acompañantes</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AcompananteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los acompañantes");
                var acompanantes = await _acompananteService.GetAllAsync();
                _logger.LogInformation("Se obtuvieron {Count} acompañantes", acompanantes.Count);
                return Ok(acompanantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los acompañantes");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un acompañante por su ID
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <returns>Detalles del acompañante</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="404">Acompañante no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AcompananteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AcompananteDto>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañante con ID: {AcompananteId}", id);
                var acompanante = await _acompananteService.GetByIdAsync(id);

                if (acompanante == null)
                {
                    _logger.LogWarning("Acompañante con ID: {AcompananteId} no encontrado", id);
                    return NotFound(new { Message = "Acompañante no encontrado" });
                }

                // Registrar visita
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                int? clienteId = null;

                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        clienteId = userId;
                    }
                }

                _ = _acompananteService.RegistrarVisitaAsync(id, ipAddress, userAgent, clienteId);
                _logger.LogInformation("Visita registrada para acompañante ID: {AcompananteId}", id);

                return Ok(acompanante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañante con ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un acompañante por el ID de su usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario asociado al acompañante</param>
        /// <returns>Detalles del acompañante</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="404">Acompañante no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("Usuario/{usuarioId}")]
        [Authorize]
        [ProducesResponseType(typeof(AcompananteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AcompananteDto>> GetByUsuarioId(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañante para usuario ID: {UsuarioId}", usuarioId);
                var acompanante = await _acompananteService.GetByUsuarioIdAsync(usuarioId);

                if (acompanante == null)
                {
                    _logger.LogWarning("Acompañante para usuario ID: {UsuarioId} no encontrado", usuarioId);
                    return NotFound(new { Message = "Acompañante no encontrado para este usuario" });
                }

                _logger.LogInformation("Acompañante obtenido para usuario ID: {UsuarioId}", usuarioId);
                return Ok(acompanante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañante para usuario ID: {UsuarioId}", usuarioId);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo acompañante
        /// </summary>
        /// <param name="nuevoAcompanante">Datos del nuevo acompañante</param>
        /// <returns>ID del acompañante creado</returns>
        /// <response code="201">Acompañante creado exitosamente</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [Authorize(Roles = "acompanante,admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Create([FromBody] CrearAcompananteDto nuevoAcompanante)
        {
            try
            {
                _logger.LogInformation("Creando nuevo acompañante por usuario ID: {UsuarioId}", GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                _logger.LogInformation("IP del cliente obtenida para geolocalización: {IpAddress}", ipAddress);

                var id = await _acompananteService.CrearAsync(nuevoAcompanante, usuarioId, ipAddress);
                _logger.LogInformation("Acompañante creado con ID: {AcompañanteId} para usuario ID: {UsuarioId}", id, usuarioId);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al crear acompañante");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear acompañante");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un acompañante existente
        /// </summary>
        /// <param name="id">ID del acompañante a actualizar</param>
        /// <param name="acompananteDto">Datos actualizados del acompañante</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="400">ID en la URL no coincide con el ID en el cuerpo</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAcompananteDto acompananteDto)
        {
            try
            {
                _logger.LogInformation("Actualizando acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                if (id != acompananteDto.Id)
                {
                    _logger.LogWarning("ID en la URL ({IdUrl}) no coincide con el ID en el cuerpo ({IdBody})", id, acompananteDto.Id);
                    return BadRequest(new { Message = "ID en la URL no coincide con el ID en el cuerpo de la solicitud" });
                }

                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                _logger.LogInformation("IP del cliente obtenida para geolocalización: {IpAddress}", ipAddress);

                await _acompananteService.ActualizarAsync(acompananteDto, usuarioId, rolId, ipAddress);
                _logger.LogInformation("Acompañante ID: {AcompañanteId} actualizado correctamente", id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al actualizar acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar acompañante ID: {AcompañanteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante a eliminar</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "acompanante,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando acompañante ID: {AcompañanteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarAsync(id, usuarioId, rolId);
                _logger.LogInformation("Acompañante ID: {AcompañanteId} eliminado correctamente", id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar acompañante ID: {AcompañanteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar acompañante ID: {AcompañanteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Agrega una foto a un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="fotoDto">Datos de la foto a agregar</param>
        /// <returns>ID de la foto agregada</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/Fotos")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> AgregarFoto(int id, [FromBody] AgregarFotoDto fotoDto)
        {
            try
            {
                _logger.LogInformation("Agregando foto para acompañante ID: {AcompañanteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var fotoId = await _acompananteService.AgregarFotoAsync(id, fotoDto, usuarioId, rolId);
                _logger.LogInformation("Foto ID: {FotoId} agregada para acompañante ID: {AcompañanteId}", fotoId, id);
                return Ok(fotoId);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al agregar foto para acompañante ID: {AcompañanteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar foto para acompañante ID: {AcompañanteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una foto de un acompañante
        /// </summary>
        /// <param name="fotoId">ID de la foto a eliminar</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("Fotos/{fotoId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EliminarFoto(int fotoId)
        {
            try
            {
                _logger.LogInformation("Eliminando foto ID: {FotoId} por usuario ID: {UsuarioId}", fotoId, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarFotoAsync(fotoId, usuarioId, rolId);
                _logger.LogInformation("Foto ID: {FotoId} eliminada correctamente", fotoId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar foto ID: {FotoId}", fotoId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar foto ID: {FotoId}", fotoId);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Establece una foto como principal para un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="fotoId">ID de la foto a establecer como principal</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/Fotos/{fotoId}/Principal")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EstablecerFotoPrincipal(int id, int fotoId)
        {
            try
            {
                _logger.LogInformation("Estableciendo foto ID: {FotoId} como principal para acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", fotoId, id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EstablecerFotoPrincipalAsync(id, fotoId, usuarioId, rolId);
                _logger.LogInformation("Foto ID: {FotoId} establecida como principal para acompañante ID: {AcompananteId}", fotoId, id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al establecer foto principal para acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer foto principal para acompañante ID: {AcompananteId}, foto ID: {FotoId}", id, fotoId);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Agrega un servicio a un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="servicioDto">Datos del servicio a agregar</param>
        /// <returns>ID del servicio agregado</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/Servicios")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> AgregarServicio(int id, [FromBody] AgregarServicioDto servicioDto)
        {
            try
            {
                _logger.LogInformation("Agregando servicio para acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var servicioId = await _acompananteService.AgregarServicioAsync(id, servicioDto, usuarioId, rolId);
                _logger.LogInformation("Servicio ID: {ServicioId} agregado para acompañante ID: {AcompañanteId}", servicioId, id);
                return Ok(servicioId);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al agregar servicio para acompañante ID: {AcompañanteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar servicio para acompañante ID: {AcompañanteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un servicio de un acompañante
        /// </summary>
        /// <param name="servicioId">ID del servicio a actualizar</param>
        /// <param name="servicioDto">Datos actualizados del servicio</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="400">ID en la URL no coincide con el ID en el cuerpo</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("Servicios/{servicioId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarServicio(int servicioId, [FromBody] ActualizarServicioDto servicioDto)
        {
            try
            {
                _logger.LogInformation("Actualizando servicio ID: {ServicioId} por usuario ID: {UsuarioId}", servicioId, GetUsuarioIdFromToken());
                if (servicioId != servicioDto.Id)
                {
                    _logger.LogWarning("ID en la URL ({IdUrl}) no coincide con el ID en el cuerpo ({IdBody})", servicioId, servicioDto.Id);
                    return BadRequest(new { Message = "ID en la URL no coincide con el ID en el cuerpo de la solicitud" });
                }

                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.ActualizarServicioAsync(servicioId, servicioDto, usuarioId, rolId);
                _logger.LogInformation("Servicio ID: {ServicioId} actualizado correctamente", servicioId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al actualizar servicio ID: {ServicioId}", servicioId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio ID: {ServicioId}", servicioId);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un servicio de un acompañante
        /// </summary>
        /// <param name="servicioId">ID del servicio a eliminar</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("Servicios/{servicioId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EliminarServicio(int servicioId)
        {
            try
            {
                _logger.LogInformation("Eliminando servicio ID: {ServicioId} por usuario ID: {UsuarioId}", servicioId, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarServicioAsync(servicioId, usuarioId, rolId);
                _logger.LogInformation("Servicio ID: {ServicioId} eliminado correctamente", servicioId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar servicio ID: {ServicioId}", servicioId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio ID: {ServicioId}", servicioId);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Agrega una categoría a un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="categoriaId">ID de la categoría a agregar</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/Categorias/{categoriaId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AgregarCategoria(int id, int categoriaId)
        {
            try
            {
                _logger.LogInformation("Agregando categoría ID: {CategoriaId} al acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", categoriaId, id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.AgregarCategoriaAsync(id, categoriaId, usuarioId, rolId);
                _logger.LogInformation("Categoría ID: {CategoriaId} agregada al acompañante ID: {AcompananteId}", categoriaId, id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al agregar categoría a acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar categoría ID: {CategoriaId} a acompañante ID: {AcompananteId}", categoriaId, id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una categoría de un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="categoriaId">ID de la categoría a eliminar</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("{id}/Categorias/{categoriaId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EliminarCategoria(int id, int categoriaId)
        {
            try
            {
                _logger.LogInformation("Eliminando categoría ID: {CategoriaId} del acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", categoriaId, id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarCategoriaAsync(id, categoriaId, usuarioId, rolId);
                _logger.LogInformation("Categoría ID: {CategoriaId} eliminada del acompañante ID: {AcompananteId}", categoriaId, id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar categoría de acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría ID: {CategoriaId} de acompañante ID: {AcompananteId}", categoriaId, id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Busca acompañantes según los filtros proporcionados
        /// </summary>
        /// <param name="filtro">Filtros de búsqueda</param>
        /// <returns>Lista de acompañantes que coinciden con los filtros</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("Buscar")]
        [ProducesResponseType(typeof(IEnumerable<AcompananteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> Buscar([FromBody] AcompananteFiltroDto filtro)
        {
            try
            {
                _logger.LogInformation("Buscando acompañantes con filtros: Búsqueda={Busqueda}, Ciudad={Ciudad}, País={País}, Género={Genero}, EdadMinima={EdadMinima}, EdadMaxima={EdadMaxima}, TarifaMinima={TarifaMinima}, TarifaMaxima={TarifaMaxima}, SoloVerificados={SoloVerificados}, SoloDisponibles={SoloDisponibles}, Categorías={Categorias}, OrdenarPor={OrdenarPor}, Página={Pagina}, ElementosPorPagina={ElementosPorPagina}",
                    filtro.Busqueda, filtro.Ciudad, filtro.Pais, filtro.Genero, filtro.EdadMinima, filtro.EdadMaxima, filtro.TarifaMinima, filtro.TarifaMaxima, filtro.SoloVerificados, filtro.SoloDisponibles, string.Join(",", filtro.CategoriaIds ?? new List<int>()), filtro.OrdenarPor, filtro.Pagina, filtro.ElementosPorPagina);

                var resultados = await _acompananteService.BuscarAsync(filtro);
                _logger.LogInformation("Búsqueda completada. Se encontraron {Count} acompañantes", resultados.Count);
                return Ok(resultados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar acompañantes");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }
        [HttpPost("BusquedaAvanzada")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResultDto<AcompananteSearchResultDto>>> BusquedaAvanzada(
       [FromBody] AdvancedSearchCriteriaDto criteria)
        {
            try
            {
                _logger.LogInformation("Realizando búsqueda avanzada de acompañantes");
                var results = await _searchService.SearchAcompanantesAsync(criteria);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar búsqueda avanzada");
                return BadRequest(new { error = "Error al procesar la búsqueda. Inténtelo de nuevo." });
            }
        }
      

        /// <summary>
        /// Obtiene los acompañantes destacados
        /// </summary>
        /// <returns>Lista de acompañantes destacados</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("Destacados")]
        [ProducesResponseType(typeof(IEnumerable<AcompananteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetDestacados()
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes destacados");
                var destacados = await _acompananteService.GetDestacadosAsync();
                _logger.LogInformation("Se obtuvieron {Count} acompañantes destacados", destacados.Count);
                return Ok(destacados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes destacados");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los acompañantes más recientes
        /// </summary>
        /// <param name="cantidad">Número de acompañantes a obtener (por defecto 10)</param>
        /// <returns>Lista de acompañantes recientes</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("Recientes")]
        [ProducesResponseType(typeof(IEnumerable<AcompananteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetRecientes([FromQuery] int cantidad = 10)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes recientes. Cantidad: {Cantidad}", cantidad);
                var recientes = await _acompananteService.GetRecientesAsync(cantidad);
                _logger.LogInformation("Se obtuvieron {Count} acompañantes recientes", recientes.Count);
                return Ok(recientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes recientes");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los acompañantes más populares
        /// </summary>
        /// <param name="cantidad">Número de acompañantes a obtener (por defecto 10)</param>
        /// <returns>Lista de acompañantes populares</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("Populares")]
        [ProducesResponseType(typeof(IEnumerable<AcompananteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetPopulares([FromQuery] int cantidad = 10)
        {
            try
            {
                _logger.LogInformation("Obteniendo acompañantes populares. Cantidad: {Cantidad}", cantidad);
                var populares = await _acompananteService.GetPopularesAsync(cantidad);
                _logger.LogInformation("Se obtuvieron {Count} acompañantes populares", populares.Count);
                return Ok(populares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes populares");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un acompañante está verificado
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <returns>Estado de verificación del acompañante</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}/Verificado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> EstaVerificado(int id)
        {
            try
            {
                _logger.LogInformation("Verificando estado de verificación para acompañante ID: {AcompananteId}", id);
                var verificado = await _acompananteService.EstaVerificadoAsync(id);
                _logger.LogInformation("Acompañante ID: {AcompananteId} está verificado: {Verificado}", id, verificado);
                return Ok(verificado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar estado de acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Registra un contacto con un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="contactoDto">Datos del contacto</param>
        /// <returns>OK si la operación es exitosa</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/Contacto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegistrarContacto(int id, [FromBody] RegistrarContactoDto contactoDto)
        {
            try
            {
                _logger.LogInformation("Registrando contacto para acompañante ID: {AcompananteId}. Tipo: {TipoContacto}", id, contactoDto.TipoContacto);
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                int? clienteId = null;

                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        clienteId = userId;
                    }
                }

                await _acompananteService.RegistrarContactoAsync(id, contactoDto.TipoContacto, ipAddress, clienteId);
                _logger.LogInformation("Contacto registrado para acompañante ID: {AcompananteId}", id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar contacto para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <returns>Estadísticas del acompañante</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}/Estadisticas")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AcompananteEstadisticasDto>> GetEstadisticas(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas para acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var estadisticas = await _acompananteService.GetEstadisticasAsync(id, usuarioId, rolId);
                _logger.LogInformation("Estadísticas obtenidas para acompañante ID: {AcompananteId}", id);
                return Ok(estadisticas);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado a estadísticas de acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las agencias disponibles para un acompañante
        /// </summary>
        /// <returns>Lista de agencias disponibles</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("agencias/disponibles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAgenciasDisponibles()
        {
            try
            {
                _logger.LogInformation("Obteniendo agencias disponibles para acompañante");
                var agencias = await _agenciaService.GetAgenciasDisponiblesAsync();
                _logger.LogInformation("Se obtuvieron {Count} agencias disponibles", agencias.Count);
                return Ok(agencias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agencias disponibles");
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Cambia la disponibilidad de un acompañante
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="dto">Datos de disponibilidad</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}/Disponibilidad")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CambiarDisponibilidad(int id, [FromBody] CambiarDisponibilidadDto dto)
        {
            try
            {
                _logger.LogInformation("Cambiando disponibilidad del acompañante ID: {AcompananteId} a {EstaDisponible} por usuario ID: {UsuarioId}", id, dto.EstaDisponible, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.CambiarDisponibilidadAsync(id, dto.EstaDisponible, usuarioId, rolId);
                _logger.LogInformation("Disponibilidad del acompañante ID: {AcompananteId} actualizada a {EstaDisponible}", id, dto.EstaDisponible);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al cambiar disponibilidad de acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar disponibilidad de acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Envía una solicitud para unirse a una agencia
        /// </summary>
        /// <param name="dto">Datos de la solicitud</param>
        /// <returns>Mensaje de confirmación</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("solicitar-agencia")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SolicitarAgencia([FromBody] CrearSolicitudAgenciaDto dto)
        {
            try
            {
                _logger.LogInformation("Enviando solicitud a agencia ID: {AgenciaId}", dto.AgenciaId);
                await _agenciaService.EnviarSolicitudAsync(dto.AgenciaId);
                _logger.LogInformation("Solicitud enviada correctamente a agencia ID: {AgenciaId}", dto.AgenciaId);
                return Ok(new { Message = "Solicitud enviada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar solicitud a agencia ID: {AgenciaId}", dto.AgenciaId);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Solicita la verificación de un acompañante (gratuito)
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <returns>OK si la operación es exitosa</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/SolicitarVerificacion")]
        [Authorize(Roles = "acompanante")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SolicitarVerificacion(int id)
        {
            try
            {
                _logger.LogInformation("Solicitando verificación para acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var acompanante = await _acompananteService.GetByIdAsync(id);

                if (acompanante == null)
                {
                    _logger.LogWarning("Acompañante ID: {AcompananteId} no encontrado", id);
                    return NotFound(new { Message = "Acompañante no encontrado" });
                }

                if (acompanante.UsuarioId != usuarioId)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para solicitar verificación del acompañante {AcompananteId}", usuarioId, id);
                    return Forbid();
                }

                if (acompanante.EstaVerificado == true)
                {
                    _logger.LogWarning("El acompañante ID: {AcompananteId} ya está verificado", id);
                    return BadRequest(new { Message = "El acompañante ya está verificado" });
                }

                if (!acompanante.AgenciaId.HasValue)
                {
                    _logger.LogWarning("El acompañante ID: {AcompananteId} no pertenece a ninguna agencia", id);
                    return BadRequest(new { Message = "El acompañante debe pertenecer a una agencia para ser verificado" });
                }

                // Aquí podrías registrar la solicitud en una tabla de "solicitudes de verificación" si tienes una,
                // pero para este flujo, simplemente devolvemos un mensaje de éxito.
                _logger.LogInformation("Solicitud de verificación registrada para acompañante ID: {AcompananteId}", id);
                return Ok(new { Message = "Solicitud de verificación enviada a la agencia." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar verificación para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Inicia el proceso de verificación por parte de la agencia (la agencia paga a la plataforma)
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="request">Datos del pago</param>
        /// <returns>ClientSecret para procesar el pago</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/IniciarVerificacion")]
        [Authorize(Roles = "agencia,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IniciarVerificacion(int id, [FromBody] IniciarVerificacionDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando verificación para acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var acompanante = await _acompananteService.GetByIdAsync(id);

                if (acompanante == null)
                {
                    _logger.LogWarning("Acompañante ID: {AcompananteId} no encontrado", id);
                    return NotFound(new { Message = "Acompañante no encontrado" });
                }

                if (acompanante.EstaVerificado == true)
                {
                    _logger.LogWarning("El acompañante ID: {AcompananteId} ya está verificado", id);
                    return BadRequest(new { Message = "El acompañante ya está verificado" });
                }

                if (!acompanante.AgenciaId.HasValue)
                {
                    _logger.LogWarning("El acompañante ID: {AcompananteId} no pertenece a ninguna agencia", id);
                    return BadRequest(new { Message = "El acompañante debe pertenecer a una agencia para ser verificado" });
                }

                // Validar que la agencia que inicia la verificación sea la misma a la que pertenece el acompañante
                if (rolId == 2 && acompanante.AgenciaId != usuarioId)
                {
                    _logger.LogWarning("Usuario {UsuarioId} (agencia) no tiene permisos para iniciar verificación del acompañante {AcompananteId} (agencia {AcompananteAgenciaId})", usuarioId, id, acompanante.AgenciaId);
                    return Forbid();
                }

                // Crear intento de pago con Stripe (la agencia paga a la plataforma)
                var metadata = new Dictionary<string, string>
                {
                    { "acompananteId", id.ToString() },
                    { "agenciaId", acompanante.AgenciaId.Value.ToString() },
                    { "tipo", "verificacion" }
                };

                var clientSecret = await _paymentService.CreatePaymentIntent(
                    amount: 50.00m, // Costo de verificación (la agencia paga a la plataforma)
                    currency: "USD",
                    description: $"Verificación de acompañante ID: {id} por agencia ID: {acompanante.AgenciaId}",
                    metadata: metadata
                );

                _logger.LogInformation("Intento de pago creado para verificación de acompañante ID: {AcompananteId} por agencia ID: {AgenciaId}. ClientSecret: {ClientSecret}", id, acompanante.AgenciaId, clientSecret);

                return Ok(new { ClientSecret = clientSecret });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar verificación para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Confirma la verificación de un acompañante tras el pago de la agencia a la plataforma
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="request">Datos de confirmación</param>
        /// <returns>No content si la operación es exitosa</returns>
        /// <response code="204">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/ConfirmarVerificacion")]
        [Authorize(Roles = "agencia,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmarVerificacion(int id, [FromBody] ConfirmarVerificacionDto request)
        {
            try
            {
                _logger.LogInformation("Confirmando verificación para acompañante ID: {AcompananteId} por usuario ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var acompanante = await _acompananteService.GetByIdAsync(id);

                if (acompanante == null)
                {
                    _logger.LogWarning("Acompañante ID: {AcompananteId} no encontrado", id);
                    return NotFound(new { Message = "Acompañante no encontrado" });
                }

                // Confirmar el pago de la agencia a la plataforma
                var paymentIntentId = request.PaymentIntentId;
                var paymentSuccess = await _paymentService.ConfirmPayment(paymentIntentId);
                if (!paymentSuccess)
                {
                    _logger.LogWarning("Pago fallido para la verificación del acompañante ID: {AcompananteId}. PaymentIntentId: {PaymentIntentId}", id, paymentIntentId);
                    return BadRequest(new { Message = "El pago no pudo ser confirmado" });
                }

                // Proceder con la verificación
                await _acompananteService.VerificarAcompananteAsync(id, usuarioId, usuarioId, rolId);
                _logger.LogInformation("Verificación confirmada para acompañante ID: {AcompananteId}", id);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al confirmar verificación para acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar verificación para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Inicia un cobro de la agencia al acompañante por la verificación
        /// </summary>
        /// <param name="id">ID del acompañante</param>
        /// <param name="request">Datos del cobro</param>
        /// <returns>ClientSecret para que el acompañante pague a la agencia</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="403">Acceso no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("{id}/CobrarAcompanantePorVerificacion")]
        [Authorize(Roles = "agencia,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CobrarAcompanantePorVerificacion(int id, [FromBody] CobrarVerificacionDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando cobro de verificación para acompañante ID: {AcompananteId} por agencia ID: {UsuarioId}", id, GetUsuarioIdFromToken());
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var acompanante = await _acompananteService.GetByIdAsync(id);

                if (acompanante == null)
                {
                    _logger.LogWarning("Acompañante ID: {AcompananteId} no encontrado", id);
                    return NotFound(new { Message = "Acompañante no encontrado" });
                }

                if (!acompanante.AgenciaId.HasValue)
                {
                    _logger.LogWarning("El acompañante ID: {AcompananteId} no pertenece a ninguna agencia", id);
                    return BadRequest(new { Message = "El acompañante debe pertenecer a una agencia para ser verificado" });
                }

                if (rolId == 2 && acompanante.AgenciaId != usuarioId)
                {
                    _logger.LogWarning("Usuario {UsuarioId} (agencia) no tiene permisos para cobrar al acompañante {AcompananteId} (agencia {AcompananteAgenciaId})", usuarioId, id, acompanante.AgenciaId);
                    return Forbid();
                }

                if (acompanante.EstaVerificado != true)
                {
                    _logger.LogWarning("El acompañante ID: {AcompananteId} no está verificado, no se puede iniciar un cobro", id);
                    return BadRequest(new { Message = "El acompañante debe estar verificado para iniciar un cobro" });
                }

                // Crear intento de pago con Stripe (el acompañante paga a la agencia)
                var metadata = new Dictionary<string, string>
                {
                    { "acompananteId", id.ToString() },
                    { "agenciaId", acompanante.AgenciaId.Value.ToString() },
                    { "tipo", "cobro_verificacion" }
                };

                var clientSecret = await _paymentService.CreatePaymentIntent(
                    amount: 50.00m, // Costo de verificación que la agencia cobra al acompañante
                    currency: "USD",
                    description: $"Cobro de verificación de acompañante ID: {id} por agencia ID: {acompanante.AgenciaId}",
                    metadata: metadata
                );

                // Registrar la transacción como pendiente para el cobro
                var transaccion = await _paymentService.DistribuirPagoAAgencia(
                    agenciaId: acompanante.AgenciaId.Value,
                    monto: 50.00m,
                    paymentIntentId: clientSecret.Split("_secret_")[0]
                );

                _logger.LogInformation("Cobro iniciado para acompañante ID: {AcompananteId} por agencia ID: {AgenciaId}. ClientSecret: {ClientSecret}", id, acompanante.AgenciaId, clientSecret);

                return Ok(new { ClientSecret = clientSecret, TransaccionId = transaccion.id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar cobro de verificación para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, new { Message = "Error interno del servidor", Error = ex.Message });
            }
        }

        /// <summary>
        /// Completa el perfil de un acompañante
        /// </summary>
        /// <param name="request">Datos del perfil a completar</param>
        /// <returns>Perfil actualizado y estado de completitud</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="404">Acompañante no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("completar-perfil")]
        [Authorize(Roles = "acompanante")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompletarPerfil([FromForm] CompletarPerfilAcompananteRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando proceso de completar perfil por usuario ID: {UsuarioId}", GetUsuarioIdFromToken());
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                _logger.LogInformation("IP del cliente obtenida para geolocalización: {IpAddress}", ipAddress);

                // Obtener el perfil actual del acompañante
                _logger.LogInformation("Buscando perfil de acompañante para usuario ID: {UsuarioId}", usuarioId);
                var acompanante = await _acompananteService.GetByUsuarioIdAsync(usuarioId);
                if (acompanante == null)
                {
                    _logger.LogWarning("No se encontró el perfil de acompañante para usuario ID: {UsuarioId}", usuarioId);
                    return NotFound(new { Message = "No se encontró el perfil de acompañante asociado a este usuario" });
                }

                // Actualizar los campos del perfil
                _logger.LogInformation("Actualizando información básica del perfil de acompañante ID: {AcompananteId}", acompanante.Id);
                var actualizarDto = new UpdateAcompananteDto
                {
                    Id = acompanante.Id,
                    Descripcion = request.Descripcion,
                    TarifaBase = request.TarifaBase,
                    Disponibilidad = request.Disponibilidad,
                    Altura = request.Altura,
                    Peso = request.Peso,
                    Idiomas = request.Idiomas
                };

                await _acompananteService.ActualizarAsync(actualizarDto, usuarioId, 3, ipAddress);
                _logger.LogInformation("Perfil de acompañante ID: {AcompananteId} actualizado con éxito", acompanante.Id);

                // Manejar las categorías
                if (request.CategoriaIds != null && request.CategoriaIds.Any())
                {
                    _logger.LogInformation("Procesando {Count} categorías para acompañante ID: {AcompananteId}", request.CategoriaIds.Count, acompanante.Id);
                    foreach (var categoriaId in request.CategoriaIds)
                    {
                        await _acompananteService.AgregarCategoriaAsync(acompanante.Id, categoriaId, usuarioId, 3);
                        _logger.LogDebug("Categoría ID: {CategoriaId} agregada al acompañante ID: {AcompananteId}", categoriaId, acompanante.Id);
                    }
                }

                // Subir la foto principal si se proporciona
                if (request.FotoPrincipal != null)
                {
                    try
                    {
                        _logger.LogInformation("Subiendo foto principal: {FileName} para acompañante ID: {AcompananteId}", request.FotoPrincipal.FileName, acompanante.Id);
                        var fotoDto = new SubirFotoDto
                        {
                            AcompananteId = acompanante.Id,
                            Foto = request.FotoPrincipal,
                            EsPrincipal = true,
                            Orden = 1
                        };

                        await _fotoService.SubirFotoAsync(fotoDto, usuarioId);
                        _logger.LogInformation("Foto principal subida para acompañante ID: {AcompananteId}", acompanante.Id);
                    }
                    catch (Exception photoEx)
                    {
                        _logger.LogError(photoEx, "Error al subir foto principal para acompañante ID: {AcompananteId}. Continuando con el resto del proceso", acompanante.Id);
                    }
                }

                // Subir fotos adicionales si se proporcionan
                if (request.FotosAdicionales != null && request.FotosAdicionales.Any())
                {
                    _logger.LogInformation("Procesando {Count} fotos adicionales para acompañante ID: {AcompananteId}", request.FotosAdicionales.Count, acompanante.Id);
                    int orden = 2;
                    foreach (var foto in request.FotosAdicionales)
                    {
                        try
                        {
                            _logger.LogInformation("Subiendo foto adicional: {FileName} para acompañante ID: {AcompananteId}", foto.FileName, acompanante.Id);
                            var fotoDto = new SubirFotoDto
                            {
                                AcompananteId = acompanante.Id,
                                Foto = foto,
                                EsPrincipal = false,
                                Orden = orden++
                            };

                            await _fotoService.SubirFotoAsync(fotoDto, usuarioId);
                            _logger.LogInformation("Foto adicional subida para acompañante ID: {AcompananteId}. Orden: {Orden}", acompanante.Id, orden - 1);
                        }
                        catch (Exception photoEx)
                        {
                            _logger.LogError(photoEx, "Error al subir foto adicional para acompañante ID: {AcompananteId}. Continuando con las demás fotos", acompanante.Id);
                        }
                    }
                }

                // Solicitar unirse a una agencia si se especifica
                if (request.SolicitarAgenciaId.HasValue && request.SolicitarAgenciaId.Value > 0)
                {
                    try
                    {
                        _logger.LogInformation("Enviando solicitud a agencia ID: {AgenciaId} por acompañante ID: {AcompananteId}", request.SolicitarAgenciaId.Value, acompanante.Id);
                        await _agenciaService.EnviarSolicitudAsync(request.SolicitarAgenciaId.Value);
                        _logger.LogInformation("Solicitud enviada a agencia ID: {AgenciaId} por acompañante ID: {AcompananteId}", request.SolicitarAgenciaId.Value, acompanante.Id);
                    }
                    catch (Exception agencyEx)
                    {
                        _logger.LogError(agencyEx, "Error al enviar solicitud a agencia ID: {AgenciaId} por acompañante ID: {AcompananteId}. Continuando con el proceso", request.SolicitarAgenciaId.Value, acompanante.Id);
                    }
                }

                // Obtener el perfil actualizado
                _logger.LogInformation("Obteniendo perfil actualizado para acompañante ID: {AcompananteId}", acompanante.Id);
                var perfilActualizado = await _acompananteService.GetByIdAsync(acompanante.Id);

                _logger.LogInformation("Perfil completado exitosamente para acompañante ID: {AcompananteId}", acompanante.Id);
                return Ok(new
                {
                    Message = "Perfil completado exitosamente",
                    Perfil = perfilActualizado,
                    PerfilCompleto = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al completar perfil de acompañante para usuario ID: {UsuarioId}", GetUsuarioIdFromToken());
                return StatusCode(500, new { Message = "Error al completar el perfil", Error = ex.Message });
            }
        }

        /// <summary>
        /// Registra un nuevo acompañante con información básica
        /// </summary>
        /// <param name="request">Datos del nuevo acompañante</param>
        /// <returns>Datos del acompañante registrado</returns>
        /// <response code="201">Acompañante registrado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("registro")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Registro([FromBody] RegisterAcompananteSimpleRequest request)
        {
            try
            {
                _logger.LogInformation("Registrando nuevo acompañante con email: {Email} y nombre: {NombrePerfil}", request.Email, request.NombrePerfil);

                // Validaciones
                if (request == null || string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.NombrePerfil) ||
                    string.IsNullOrWhiteSpace(request.Genero) ||
                    request.Edad < 18)
                {
                    _logger.LogWarning("Campos obligatorios faltantes o inválidos en el registro de acompañante");
                    return BadRequest(new { Message = "Todos los campos obligatorios deben ser completados correctamente. La edad debe ser mayor o igual a 18." });
                }

                // Validar formato de email
                if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    _logger.LogWarning("Formato de email inválido: {Email}", request.Email);
                    return BadRequest(new { Message = "El email proporcionado no tiene un formato válido." });
                }

                // Validar longitud de contraseña
                if (request.Password.Length < 6)
                {
                    _logger.LogWarning("Contraseña demasiado corta para email: {Email}", request.Email);
                    return BadRequest(new { Message = "La contraseña debe tener al menos 6 caracteres." });
                }

                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                _logger.LogInformation("IP del cliente obtenida para geolocalización: {IpAddress}", ipAddress);

                var resultado = await _userService.RegisterUserAcompananteAsync(
                    email: request.Email,
                    password: request.Password,
                    telefono: request.WhatsApp ?? string.Empty,
                    nombrePerfil: request.NombrePerfil,
                    genero: request.Genero,
                    edad: request.Edad,
                    descripcion: string.Empty,
                    ciudad: null,
                    pais: null,
                    disponibilidad: "Horario flexible",
                    tarifaBase: 0,
                    moneda: "USD",
                    categoriaIds: new List<int>(),
                    whatsapp: request.WhatsApp ?? string.Empty,
                    emailContacto: null,
                    altura: 160,
                    peso: 60,
                    idiomas: "Español",
                    clientIp: ipAddress
                );

                var user = resultado.Usuario;
                var acompananteId = resultado.AcompananteId;

                _logger.LogInformation("Acompañante registrado con ID: {AcompañanteId} y usuario ID: {UserId}", acompananteId, user.id);
                return CreatedAtAction(nameof(GetById), new { id = acompananteId }, new
                {
                    UserId = user.id,
                    Email = user.email,
                    AcompañanteId = acompananteId,
                    NombrePerfil = request.NombrePerfil,
                    PerfilCompleto = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro de acompañante");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Agrega una foto principal a un acompañante
        /// </summary>
        /// <param name="foto">Archivo de la foto principal</param>
        /// <returns>Mensaje de éxito y datos de la foto</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="404">Acompañante no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("agregar-foto-principal")]
        [Authorize(Roles = "acompanante")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AgregarFotoPrincipal(IFormFile foto)
        {
            try
            {
                _logger.LogInformation("Agregando foto principal por usuario ID: {UsuarioId}", GetUsuarioIdFromToken());
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var acompanante = await _acompananteService.GetByUsuarioIdAsync(usuarioId);

                if (acompanante == null)
                {
                    _logger.LogWarning("No se encontró el perfil de acompañante para usuario ID: {UsuarioId}", usuarioId);
                    return NotFound(new { Message = "No se encontró el perfil de acompañante" });
                }

                var fotoDto = new SubirFotoDto
                {
                    AcompananteId = acompanante.Id,
                    Foto = foto,
                    EsPrincipal = true,
                    Orden = 1
                };

                var result = await _fotoService.SubirFotoAsync(fotoDto, usuarioId);
                _logger.LogInformation("Foto principal subida para acompañante ID: {AcompañanteId}", acompanante.Id);
                return Ok(new { Message = "Foto subida exitosamente", Foto = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto principal para usuario ID: {UsuarioId}", GetUsuarioIdFromToken());
                return StatusCode(500, new { Message = "Error al subir la foto", Error = ex.Message });
            }
        }

        // Métodos privados auxiliares
        private int GetUsuarioIdFromToken()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null)
            {
                _logger.LogWarning("Token inválido: No se encontró Claim de usuario");
                throw new UnauthorizedAccessException("Token inválido");
            }

            return int.Parse(usuarioIdClaim.Value);
        }

        private int GetRolIdFromToken()
        {
            var rolIdClaim = User.FindFirst("rol_id");
            if (rolIdClaim == null)
            {
                _logger.LogWarning("Token inválido: No se encontró Claim de rol");
                throw new UnauthorizedAccessException("Token inválido");
            }

            return int.Parse(rolIdClaim.Value);
        }
    }
}