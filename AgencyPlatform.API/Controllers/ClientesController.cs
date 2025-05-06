using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.DTOs.Sorteos;
using AgencyPlatform.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AgencyPlatform.Application.Interfaces.Services.Cliente;
using System.ComponentModel.DataAnnotations;

namespace AgencyPlatform.API.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            IClienteService clienteService,
            ILogger<ClientesController> logger)
        {
            _clienteService = clienteService ?? throw new ArgumentNullException(nameof(clienteService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene los datos de un cliente por su ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ClienteDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ClienteDto>> GetById(int id)
        {
            try
            {
                var cliente = await _clienteService.GetByIdAsync(id);
                return Ok(cliente);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }
        /// <summary>
        /// Registra un nuevo cliente en la plataforma
        /// </summary>
        [HttpPost("registro")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ClienteDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> RegistrarCliente([FromBody] RegistroClienteDto dto)
        {
            try
            {
                _logger.LogInformation("Iniciando registro de cliente con email: {Email}", dto.Email);
                var cliente = await _clienteService.RegistrarClienteAsync(dto);
                _logger.LogInformation("Cliente registrado exitosamente. ID: {ClienteId}", cliente.Id);

                return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Error de validación durante registro de cliente: {Error}", ex.Message);
                return BadRequest(new ProblemDetails { Detail = ex.Message });
            }
            catch (DuplicateEntityException ex)
            {
                _logger.LogWarning("Intento de registro con datos duplicados: {Error}", ex.Message);
                return Conflict(new ProblemDetails { Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado durante registro de cliente");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ProblemDetails { Detail = "Error interno del servidor durante el registro" });
            }
        }

        /// <summary>
        /// Obtiene los datos detallados de un cliente por su ID
        /// </summary>
        [HttpGet("{id}/detail")]
        [Authorize]
        [ProducesResponseType(typeof(ClienteDetailDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetClienteDetail(int id)
        {
            try
            {
                var clienteDetail = await _clienteService.GetClienteDetailAsync(id);
                return Ok(clienteDetail);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el dashboard personalizado del cliente
        /// </summary>
        [HttpGet("{id}/dashboard")]
        [Authorize]
        [ProducesResponseType(typeof(ClienteDashboardDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDashboard(int id)
        {
            try
            {
                var dashboard = await _clienteService.GetDashboardAsync(id);
                return Ok(dashboard);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de puntos del cliente
        /// </summary>
        [HttpGet("{id}/puntos/historial")]
        [Authorize]
        [ProducesResponseType(typeof(List<MovimientoPuntosDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetHistorialPuntos(int id, [FromQuery] int cantidad = 10)
        {
            try
            {
                // Ajustado para usar el nombre correcto del método
                var historial = await _clienteService.ObtenerHistorialPuntosAsync(id, cantidad);
                return Ok(historial);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los cupones disponibles del cliente
        /// </summary>
        [HttpGet("{id}/cupones")]
        [Authorize]
        [ProducesResponseType(typeof(List<CuponClienteDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCuponesDisponibles(int id)
        {
            try
            {
                // Ajustado para usar el nombre correcto del método
                var cupones = await _clienteService.ObtenerCuponesDisponiblesAsync(id);
                return Ok(cupones);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

       

        /// <summary>
        /// Compra un paquete de cupones
        /// </summary>
        [HttpPost("{id}/compras")]
        [Authorize]
        [ProducesResponseType(typeof(CompraDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ComprarPaquete(int id, [FromBody] ComprarPaqueteDto dto)
        {
            try
            {
                var compra = await _clienteService.ComprarPaqueteAsync(id, dto);
                return Ok(compra);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
            catch (BusinessRuleViolationException ex)
            {
                return BadRequest(new ProblemDetails { Detail = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de compras del cliente
        /// </summary>
        [HttpGet("{id}/compras")]
        [Authorize]
        [ProducesResponseType(typeof(List<CompraDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetHistorialCompras(int id)
        {
            try
            {
                var compras = await _clienteService.GetHistorialComprasAsync(id);
                return Ok(compras);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Suscribe al cliente a un plan VIP
        /// </summary>
        [HttpPost("{id}/suscripcion-vip")]
        [Authorize]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SuscribirseVip(int id, [FromBody] SuscribirseVipDto dto)
        {
            try
            {
                var resultado = await _clienteService.SuscribirseVipAsync(id, dto);
                return Ok(resultado);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
            catch (BusinessRuleViolationException ex)
            {
                return BadRequest(new ProblemDetails { Detail = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Cancela la suscripción VIP del cliente
        /// </summary>
        [HttpDelete("{id}/suscripcion-vip")]
        [Authorize]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelarSuscripcionVip(int id)
        {
            try
            {
                var resultado = await _clienteService.CancelarSuscripcionVipAsync(id);
                return Ok(resultado);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
            catch (BusinessRuleViolationException ex)
            {
                return BadRequest(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si el cliente tiene suscripción VIP activa
        /// </summary>
        [HttpGet("{id}/es-vip")]
        [Authorize]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EsClienteVip(int id)
        {
            try
            {
                var esVip = await _clienteService.EsClienteVipAsync(id);
                return Ok(esVip);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Registra la visita de un cliente a un perfil de acompañante
        /// </summary>
        [HttpPost("{id}/visitas/{acompananteId}")]
        [AllowAnonymous] // Permite visitas anónimas
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RegistrarVisitaPerfil(
            int? id,
            int acompananteId,
            [FromHeader(Name = "X-Forwarded-For")] string ipVisitante,
            [FromHeader(Name = "User-Agent")] string userAgent)
        {
            try
            {
                await _clienteService.RegistrarVisitaPerfilAsync(id, acompananteId, ipVisitante, userAgent);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Registra el contacto de un cliente con un perfil de acompañante
        /// </summary>
        [HttpPost("{id}/contactos/{acompananteId}")]
        [AllowAnonymous] // Permite contactos anónimos
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RegistrarContactoPerfil(
            int? id,
            int acompananteId,
            [FromQuery] string tipoContacto,
            [FromHeader(Name = "X-Forwarded-For")] string ipContacto)
        {
            try
            {
                var resultado = await _clienteService.RegistrarContactoPerfilAsync(id, acompananteId, tipoContacto, ipContacto);
                return Ok(resultado);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Permite a un cliente participar en un sorteo
        /// </summary>
        [HttpPost("{id}/sorteos/{sorteoId}/participar")]
        [Authorize]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> ParticiparEnSorteo(int id, int sorteoId)
        {
            try
            {
                var resultado = await _clienteService.ParticiparEnSorteoAsync(id, sorteoId);
                return Ok(resultado);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
            catch (BusinessRuleViolationException ex)
            {
                return BadRequest(new ProblemDetails { Detail = ex.Message });
            }
            catch (DuplicateEntityException ex)
            {
                return Conflict(new ProblemDetails { Detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los sorteos en los que participa un cliente
        /// </summary>
        [HttpGet("{id}/sorteos")]
        [Authorize]
        [ProducesResponseType(typeof(List<SorteoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSorteosParticipando(int id)
        {
            try
            {
                var sorteos = await _clienteService.GetSorteosParticipandoAsync(id);
                return Ok(sorteos);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Detail = ex.Message });
            }
        }
    }
}