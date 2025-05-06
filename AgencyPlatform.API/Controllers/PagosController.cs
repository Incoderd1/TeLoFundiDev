using AgencyPlatform.Application.Interfaces.Services.Cliente;
using AgencyPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgencyPlatform.Application.Interfaces.Services.Cupones;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.DTOs.Payment;
using AgencyPlatform.Application.DTOs.Payments;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IClienteService _clienteService;
        private readonly IPaqueteCuponService _paqueteService;
        private readonly ILogger<PagosController> _logger;


        public PagosController(
            IPaymentService paymentService,
            IClienteService clienteService,
            IPaqueteCuponService paqueteService,
            ILogger<PagosController> logger)
        {
            _paymentService = paymentService;
            _clienteService = clienteService;
            _paqueteService = paqueteService;
            _logger = logger;
        }

        [HttpGet("paquetes")]
        public async Task<ActionResult<List<PaqueteCuponDto>>> GetPaquetesCupones()
        {
            try
            {
                var paquetes = await _paqueteService.GetAllActivosAsync();
                return Ok(paquetes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquetes de cupones");
                return BadRequest(new { error = "Error al obtener los paquetes disponibles" });
            }
        }
        [HttpPost("crear-intento-pago")]
        [Authorize]
        public async Task<ActionResult<CreatePaymentIntentResponseDto>> CrearIntentoPago(
          [FromBody] CreatePaymentIntentRequestDto dto)
        {
            try
            {
                // Obtener ID del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Usuario no identificado correctamente" });
                }

                // Obtener cliente
                var cliente = await _clienteService.GetByUsuarioIdAsync(userId);

                // Obtener paquete
                var paquete = await _paqueteService.GetByIdAsync(dto.PaqueteId);
                if (paquete == null)
                {
                    return NotFound(new { error = "Paquete no encontrado" });
                }

                // Crear intento de pago
                var clientSecret = await _paymentService.CreatePaymentIntent(
                    paquete.Precio,
                    dto.Currency ?? "usd",
                    $"Compra de paquete: {paquete.Nombre}"
                );

                return Ok(new CreatePaymentIntentResponseDto
                {
                    ClientSecret = clientSecret,
                    PaqueteId = paquete.Id,
                    Amount = paquete.Precio,
                    Currency = dto.Currency ?? "usd"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear intento de pago");
                return BadRequest(new { error = "Error al procesar la solicitud de pago" });
            }
        }
        [HttpPost("confirmar-compra")]
        [Authorize]
        public async Task<ActionResult<PaymentStatusDto>> ConfirmarCompraPaquete(
     [FromBody] ConfirmarCompraDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.PaymentIntentId))
                {
                    return BadRequest(new { error = "El PaymentIntentId es requerido para confirmar la compra" });
                }

                var status = await _paymentService.GetPaymentStatus(dto.PaymentIntentId);
                if (status.Status != "succeeded")
                {
                    return BadRequest(new { error = "El pago no ha sido completado correctamente" });
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar compra de paquete");
                return BadRequest(new { error = "Error al procesar la compra" });
            }
        }

        [HttpGet("cliente/metodos-pago")]
        [Authorize]
        public async Task<ActionResult<List<PaymentMethodDto>>> GetMetodosPago()
        {
            try
            {
                // Obtener ID del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Usuario no identificado correctamente" });
                }

                // Obtener cliente
                var cliente = await _clienteService.GetByUsuarioIdAsync(userId);

                // Aquí deberías tener una forma de obtener el customerId de Stripe
                // Por ejemplo, podría estar almacenado en el cliente
                var customerId = cliente.StripeCustomerId;

                if (string.IsNullOrEmpty(customerId))
                {
                    return Ok(new List<PaymentMethodDto>()); // Cliente sin métodos de pago
                }

                var metodosPago = await _paymentService.GetCustomerPaymentMethods(customerId);
                return Ok(metodosPago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métodos de pago");
                return BadRequest(new { error = "Error al obtener los métodos de pago" });
            }
        }
    }

}

