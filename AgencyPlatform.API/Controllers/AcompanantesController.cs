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
using AgencyPlatform.Application.DTOs.Usuarios;
using AgencyPlatform.Application;

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

     
        public AcompanantesController(
           IAcompananteService acompananteService,
           ILogger<AcompanantesController> logger, IAgenciaService agenciaService, IFotoService fotoService ,IUserService service)
        {
            _acompananteService = acompananteService;
            _logger = logger;
            _agenciaService = agenciaService;
            _fotoService = fotoService;
            _userService = service; 

        }
        /// <summary>
        /// Obtiene una lista paginada de todos los acompañantes
        /// </summary>
        /// <param name="pageNumber">Número de página (mínimo 1)</param>
        /// <param name="pageSize">Tamaño de página (entre 1 y 50)</param>
        /// <returns>Lista paginada de acompañantes</returns>
        /// <response code="200">Operación exitosa</response>
        /// <response code="500">Error interno del servidor</response>
        /// [HttpGet("Paginados")]
        [ProducesResponseType(typeof(PaginatedResultDto<AcompananteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("Paginados")]
        public async Task<ActionResult<PaginatedResultDto<AcompananteDto>>> GetPaginados(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var result = await _acompananteService.GetAllPaginatedAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes paginados");
                return StatusCode(500, "Error interno del servidor");
            }
        }


        // GET: api/Acompanantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetAll()
        {
            try
            {
                var acompanantes = await _acompananteService.GetAllAsync();
                return Ok(acompanantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los acompañantes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Acompanantes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AcompananteDto>> GetById(int id)
        {
            try
            {
                var acompanante = await _acompananteService.GetByIdAsync(id);

                if (acompanante == null)
                    return NotFound();

                // Registrar visita (solo para GET por ID)
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string? userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                int? clienteId = null;

                // Si el usuario está autenticado, obtener su ID
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        clienteId = userId;
                    }
                }

                // Registrar la visita de manera asíncrona sin esperar respuesta
                _ = _acompananteService.RegistrarVisitaAsync(id, ipAddress, userAgent, clienteId);

                return Ok(acompanante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañante con ID: {AcompananteId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Acompanantes/Usuario/5
        [HttpGet("Usuario/{usuarioId}")]
        public async Task<ActionResult<AcompananteDto>> GetByUsuarioId(int usuarioId)
        {
            try
            {
                var acompanante = await _acompananteService.GetByUsuarioIdAsync(usuarioId);

                if (acompanante == null)
                    return NotFound();

                return Ok(acompanante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañante por Usuario ID: {UsuarioId}", usuarioId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Acompanantes
        [HttpPost]
        [Authorize(Roles = "acompanante,admin")]
        public async Task<ActionResult<int>> Create([FromBody] CrearAcompananteDto nuevoAcompanante)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var id = await _acompananteService.CrearAsync(nuevoAcompanante, usuarioId);
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
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Acompanantes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAcompananteDto acompananteDto)
        {
            try
            {
                if (id != acompananteDto.Id)
                    return BadRequest("ID en la URL no coincide con el ID en el cuerpo de la solicitud");

                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.ActualizarAsync(acompananteDto, usuarioId, rolId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al actualizar acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar acompañante ID: {AcompananteId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Acompanantes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "acompanante,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarAsync(id, usuarioId, rolId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar acompañante ID: {AcompananteId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Acompanantes/5/Fotos
        [HttpPost("{id}/Fotos")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<ActionResult<int>> AgregarFoto(int id, [FromBody] AgregarFotoDto fotoDto)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var fotoId = await _acompananteService.AgregarFotoAsync(id, fotoDto, usuarioId, rolId);
                return Ok(fotoId);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al agregar foto para acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar foto para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Acompanantes/Fotos/5
        [HttpDelete("Fotos/{fotoId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> EliminarFoto(int fotoId)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarFotoAsync(fotoId, usuarioId, rolId);
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
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Acompanantes/5/Fotos/3/Principal
        [HttpPost("{id}/Fotos/{fotoId}/Principal")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> EstablecerFotoPrincipal(int id, int fotoId)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EstablecerFotoPrincipalAsync(id, fotoId, usuarioId, rolId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al establecer foto principal para acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer foto principal para acompañante ID: {AcompananteId}, foto ID: {FotoId}",
                    id, fotoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/Servicios")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<ActionResult<int>> AgregarServicio(int id, [FromBody] AgregarServicioDto servicioDto)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var servicioId = await _acompananteService.AgregarServicioAsync(id, servicioDto, usuarioId, rolId);
                return Ok(servicioId);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al agregar servicio para acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar servicio para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("Servicios/{servicioId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> ActualizarServicio(int servicioId, [FromBody] ActualizarServicioDto servicioDto)
        {
            try
            {
                if (servicioId != servicioDto.Id)
                    return BadRequest("ID en la URL no coincide con el ID en el cuerpo de la solicitud");

                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.ActualizarServicioAsync(servicioId, servicioDto, usuarioId, rolId);
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
                return StatusCode(500, "Error interno del servidor");
            }
        }


        // DELETE: api/Acompanantes/Servicios/5
        [HttpDelete("Servicios/{servicioId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> EliminarServicio(int servicioId)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarServicioAsync(servicioId, usuarioId, rolId);
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
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Acompanantes/5/Categorias/3
        [HttpPost("{id}/Categorias/{categoriaId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> AgregarCategoria(int id, int categoriaId)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.AgregarCategoriaAsync(id, categoriaId, usuarioId, rolId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al agregar categoría a acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar categoría {CategoriaId} a acompañante ID: {AcompananteId}",
                    categoriaId, id);
                return StatusCode(500, "Error interno del servidor");
            }
        }


        // DELETE: api/Acompanantes/5/Categorias/3
        [HttpDelete("{id}/Categorias/{categoriaId}")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> EliminarCategoria(int id, int categoriaId)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.EliminarCategoriaAsync(id, categoriaId, usuarioId, rolId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar categoría de acompañante ID: {AcompananteId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría {CategoriaId} de acompañante ID: {AcompananteId}",
                    categoriaId, id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Acompanantes/Buscar
        [HttpPost("Buscar")]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> Buscar([FromBody] AcompananteFiltroDto filtro)
        {
            try
            {
                var resultados = await _acompananteService.BuscarAsync(filtro);
                return Ok(resultados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar acompañantes");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        // GET: api/Acompanantes/Destacados
        [HttpGet("Destacados")]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetDestacados()
        {
            try
            {
                var destacados = await _acompananteService.GetDestacadosAsync();
                return Ok(destacados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes destacados");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Acompanantes/Recientes?cantidad=10
        [HttpGet("Recientes")]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetRecientes([FromQuery] int cantidad = 10)
        {
            try
            {
                var recientes = await _acompananteService.GetRecientesAsync(cantidad);
                return Ok(recientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes recientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Acompanantes/Populares?cantidad=10
        [HttpGet("Populares")]
        public async Task<ActionResult<IEnumerable<AcompananteDto>>> GetPopulares([FromQuery] int cantidad = 10)
        {
            try
            {
                var populares = await _acompananteService.GetPopularesAsync(cantidad);
                return Ok(populares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acompañantes populares");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Acompanantes/5/Verificado
        [HttpGet("{id}/Verificado")]
        public async Task<ActionResult<bool>> EstaVerificado(int id)
        {
            try
            {
                var verificado = await _acompananteService.EstaVerificadoAsync(id);
                return Ok(verificado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar estado de acompañante ID: {AcompananteId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Acompanantes/5/Contacto
        [HttpPost("{id}/Contacto")]
        public async Task<IActionResult> RegistrarContacto(int id, [FromBody] RegistrarContactoDto contactoDto)
        {
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                int? clienteId = null;

                // Si el usuario está autenticado, obtener su ID
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        clienteId = userId;
                    }
                }

                await _acompananteService.RegistrarContactoAsync(id, contactoDto.TipoContacto, ipAddress, clienteId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar contacto para acompañante ID: {AcompananteId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Acompanantes/5/Estadisticas
        [HttpGet("{id}/Estadisticas")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<ActionResult<AcompananteEstadisticasDto>> GetEstadisticas(int id)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                var estadisticas = await _acompananteService.GetEstadisticasAsync(id, usuarioId, rolId);
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
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpGet("agencias/disponibles")]
        //[Authorize(Roles = "acompanante")]
        public async Task<IActionResult> GetAgenciasDisponibles()
        {
            var agencias = await _agenciaService.GetAgenciasDisponiblesAsync();
            return Ok(agencias);
        }


        // PUT: api/Acompanantes/5/Disponibilidad
        [HttpPut("{id}/Disponibilidad")]
        [Authorize(Roles = "acompanante,agencia,admin")]
        public async Task<IActionResult> CambiarDisponibilidad(int id, [FromBody] CambiarDisponibilidadDto dto)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                var rolId = GetRolIdFromToken();
                await _acompananteService.CambiarDisponibilidadAsync(id, dto.EstaDisponible, usuarioId, rolId);
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
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("solicitar-agencia")]
        //[Authorize(Roles = "acompanante")]
        public async Task<IActionResult> SolicitarAgencia([FromBody] CrearSolicitudAgenciaDto dto)
        {
            await _agenciaService.EnviarSolicitudAsync(dto.AgenciaId);
            return Ok(new { Message = "Solicitud enviada correctamente." });
        }


        [HttpPost("completar-perfil")]
        [Authorize(Roles = "acompanante")]
        public async Task<IActionResult> CompletarPerfil([FromForm] CompletarPerfilAcompananteRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando proceso de completar perfil");
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // 1. Obtener el perfil actual del acompañante
                _logger.LogInformation("Buscando perfil de acompañante para usuario {UsuarioId}", usuarioId);
                var acompanante = await _acompananteService.GetByUsuarioIdAsync(usuarioId);
                if (acompanante == null)
                    return NotFound(new { Message = "No se encontró el perfil de acompañante asociado a este usuario" });

                // 2. Actualizar los campos del perfil
                _logger.LogInformation("Actualizando información básica del perfil");
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

                await _acompananteService.ActualizarAsync(actualizarDto, usuarioId, 3);

                // 3. Manejar las categorías
                if (request.CategoriaIds != null && request.CategoriaIds.Any())
                {
                    _logger.LogInformation("Procesando {Count} categorías", request.CategoriaIds.Count);
                    foreach (var categoriaId in request.CategoriaIds)
                    {
                        await _acompananteService.AgregarCategoriaAsync(acompanante.Id, categoriaId, usuarioId, 3);
                    }
                }

                // 4. Subir la foto principal si se proporciona
                if (request.FotoPrincipal != null)
                {
                    try
                    {
                        _logger.LogInformation("Subiendo foto principal: {FileName}", request.FotoPrincipal.FileName);
                        var fotoDto = new SubirFotoDto
                        {
                            AcompananteId = acompanante.Id,
                            Foto = request.FotoPrincipal,
                            EsPrincipal = true,
                            Orden = 1
                        };

                        await _fotoService.SubirFotoAsync(fotoDto, usuarioId);
                    }
                    catch (Exception photoEx)
                    {
                        _logger.LogError(photoEx, "Error al subir foto principal. Continuando con el resto del proceso");
                        // Continuar con el proceso en lugar de fallar completamente
                    }
                }

                // 5. Subir fotos adicionales si se proporcionan
                if (request.FotosAdicionales != null && request.FotosAdicionales.Any())
                {
                    _logger.LogInformation("Procesando {Count} fotos adicionales", request.FotosAdicionales.Count);
                    int orden = 2;
                    foreach (var foto in request.FotosAdicionales)
                    {
                        try
                        {
                            _logger.LogInformation("Subiendo foto adicional: {FileName}", foto.FileName);
                            var fotoDto = new SubirFotoDto
                            {
                                AcompananteId = acompanante.Id,
                                Foto = foto,
                                EsPrincipal = false,
                                Orden = orden++
                            };

                            await _fotoService.SubirFotoAsync(fotoDto, usuarioId);
                        }
                        catch (Exception photoEx)
                        {
                            _logger.LogError(photoEx, "Error al subir foto adicional. Continuando con las demás fotos");
                            // Continuar con las otras fotos
                        }
                    }
                }

                // 6. Solicitar unirse a una agencia si se especifica
                if (request.SolicitarAgenciaId.HasValue && request.SolicitarAgenciaId.Value > 0)
                {
                    try
                    {
                        _logger.LogInformation("Enviando solicitud a agencia ID: {AgenciaId}", request.SolicitarAgenciaId.Value);
                        await _agenciaService.EnviarSolicitudAsync(request.SolicitarAgenciaId.Value);
                    }
                    catch (Exception agencyEx)
                    {
                        _logger.LogError(agencyEx, "Error al enviar solicitud a agencia. Continuando con el proceso");
                        // Continuar con el proceso
                    }
                }

                // 7. Obtener el perfil actualizado
                _logger.LogInformation("Obteniendo perfil actualizado");
                var perfilActualizado = await _acompananteService.GetByIdAsync(acompanante.Id);

                return Ok(new
                {
                    Message = "Perfil completado exitosamente",
                    Perfil = perfilActualizado,
                    PerfilCompleto = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al completar perfil de acompañante");
                return StatusCode(500, new { Message = "Error al completar el perfil", Error = ex.Message });
            }
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<IActionResult> Registro([FromBody] RegisterAcompananteSimpleRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.NombrePerfil) ||
                    string.IsNullOrWhiteSpace(request.Genero) ||
                    request.Edad < 18)
                {
                    return BadRequest(new { Message = "Todos los campos obligatorios deben ser completados correctamente." });
                }

                // Asignar valores por defecto para los campos opcionales
                var resultado = await _userService.RegisterUserAcompananteAsync(
                    email: request.Email,
                    password: request.Password,
                    telefono: request.WhatsApp ?? string.Empty,
                    nombrePerfil: request.NombrePerfil,
                    genero: request.Genero,
                    edad: request.Edad,
                    descripcion: string.Empty,
                    ciudad: request.Ciudad ?? string.Empty,
                    pais: request.Pais ?? string.Empty,
                    disponibilidad: "Horario flexible",
                    tarifaBase: 0,
                    moneda: "USD",
                    categoriaIds: new List<int>(),
                    whatsapp: request.WhatsApp ?? string.Empty,
                    altura: 0,
                    peso: 0,
                    idiomas: string.Empty
                );

                var user = resultado.Usuario;
                var acompananteId = resultado.AcompananteId;

                return CreatedAtAction(nameof(GetById), new { id = acompananteId }, new
                {
                    UserId = user.id,
                    Email = user.email,
                    AcompananteId = acompananteId,
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

        [HttpPost("agregar-foto-principal")]
        [Authorize(Roles = "acompanante")]
        public async Task<IActionResult> AgregarFotoPrincipal(IFormFile foto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var acompanante = await _acompananteService.GetByUsuarioIdAsync(usuarioId);

                if (acompanante == null)
                    return NotFound(new { Message = "No se encontró el perfil de acompañante" });

                var fotoDto = new SubirFotoDto
                {
                    AcompananteId = acompanante.Id,
                    Foto = foto,
                    EsPrincipal = true,
                    Orden = 1
                };

                var result = await _fotoService.SubirFotoAsync(fotoDto, usuarioId);
                return Ok(new { Message = "Foto subida exitosamente", Foto = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        // Métodos privados auxiliares
        private int GetUsuarioIdFromToken()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null)
                throw new UnauthorizedAccessException("Token inválido");

            return int.Parse(usuarioIdClaim.Value);
        }

        private int GetRolIdFromToken()
        {
            var rolIdClaim = User.FindFirst("rol_id");
            if (rolIdClaim == null)
                throw new UnauthorizedAccessException("Token inválido");

            return int.Parse(rolIdClaim.Value);
        }
    }
}