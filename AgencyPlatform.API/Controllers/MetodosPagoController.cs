using AgencyPlatform.Application.Interfaces.Services.Acompanantes;
using AgencyPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgencyPlatform.Application.DTOs.MetodoPago;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using AgencyPlatform.Application.DTOs.Acompanantes;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetodosPagoController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IAcompananteService _acompananteService;
        private readonly ILogger<MetodosPagoController> _logger;
        private readonly IConfiguration _configuration;

        private readonly IUsuarioService _usuarioService;

        private readonly IAgenciaService _agenciaService;

        public MetodosPagoController(
            IPaymentService paymentService,
            IAcompananteService acompananteService,
            ILogger<MetodosPagoController> logger,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _acompananteService = acompananteService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("iniciar-stripe-connect")]
        [Authorize(Policy = "AcompananteOwnerOrAgencia")]
        public async Task<ActionResult> IniciarStripeConnect([FromBody] IniciarProcesoStripeDto dto)
        {
            try
            {
                // Verificar permisos
                var usuarioId = ObtenerUsuarioId();
                var acompanante = await _acompananteService.GetByIdAsync(dto.AcompananteId);

                if (acompanante == null)
                    return NotFound(new { mensaje = "Acompañante no encontrado" });

                if (acompanante.UsuarioId != usuarioId &&
                         (acompanante.AgenciaId == null || !await _agenciaService.EsPropietarioAgenciaAsync(acompanante.AgenciaId.Value, usuarioId)))

                    // Verificar si ya tiene cuenta
                    if (!string.IsNullOrEmpty(acompanante.StripeAccountId))
                {
                    var status = await _paymentService.GetConnectedAccountStatusAsync(acompanante.StripeAccountId);

                    // Si ya tiene cuenta activa, solo devolver el estado
                    if (status.PayoutsEnabled)
                        return Ok(status);

                    // Si tiene cuenta pero necesita completar onboarding
                    if (status.RequiereAccion)
                    {
                        // Generar nuevo link de onboarding
                        status.OnboardingUrl = await _paymentService.GenerateOnboardingLinkAsync(
                            acompanante.StripeAccountId,
                            dto.ReturnUrl,
                            $"{Request.Scheme}://{Request.Host}/api/MetodosPago/onboarding-refresh/{dto.AcompananteId}"
                        );

                        return Ok(status);
                    }
                }

                // Crear nueva cuenta Stripe Connect
                string email = acompanante.Email ?? (await _usuarioService.GetByIdAsync(acompanante.UsuarioId))?.Email;

                if (string.IsNullOrEmpty(email))
                    return BadRequest(new { mensaje = "El acompañante no tiene un email configurado" });

                // Crear cuenta con Stripe
                string stripeAccountId = await _paymentService.CreateConnectedAccountAsync(
                             email,
                             "express",
                             new Dictionary<string, string> { { "acompananteId", dto.AcompananteId.ToString() } }
                );

                // Actualizar el ID de cuenta en la base de datos
                await _acompananteService.ActualizarStripeAccountIdAsync(dto.AcompananteId, stripeAccountId);

                // Generar URL de onboarding
                string onboardingUrl = await _paymentService.GenerateOnboardingLinkAsync(
                    stripeAccountId,
                    dto.ReturnUrl,
                    $"{Request.Scheme}://{Request.Host}/api/MetodosPago/onboarding-refresh/{dto.AcompananteId}"
                );

                // Devolver información
                return Ok(new
                {
                    mensaje = "Proceso de configuración de pagos iniciado",
                    stripeAccountId = stripeAccountId,
                    onboardingUrl = onboardingUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar proceso Stripe Connect para acompañante {AcompananteId}", dto.AcompananteId);
                return StatusCode(500, new { mensaje = "Error al iniciar proceso de configuración de pagos" });
            }
        }
        [HttpGet("onboarding-complete/{acompananteId}")]
        [AllowAnonymous]
        public async Task<ActionResult> OnboardingComplete(int acompananteId, [FromQuery] string returnUrl)
        {
            try
            {
                var acompanante = await _acompananteService.GetByIdAsync(acompananteId);
                if (acompanante == null)
                    return NotFound();

                // Verificar el estado de la cuenta
                if (!string.IsNullOrEmpty(acompanante.StripeAccountId))
                {
                    await _paymentService.ActualizarEstadoCuentaConectadaAsync(acompanante.StripeAccountId);
                }

                // Redirigir al usuario a la URL especificada
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);

                return Ok(new { mensaje = "Configuración de pagos completada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en callback de onboarding para acompañante {AcompananteId}", acompananteId);
                return StatusCode(500, new { mensaje = "Error al procesar el onboarding" });
            }
        }

        [HttpGet("onboarding-refresh/{acompananteId}")]
        [AllowAnonymous]
        public async Task<ActionResult> OnboardingRefresh(int acompananteId, [FromQuery] string returnUrl)
        {
            try
            {
                var acompanante = await _acompananteService.GetByIdAsync(acompananteId);
                if (acompanante == null)
                    return NotFound();

                if (string.IsNullOrEmpty(acompanante.StripeAccountId))
                    return BadRequest(new { mensaje = "El acompañante no tiene una cuenta Stripe asociada" });

                // Generar nuevo link de onboarding
                string onboardingUrl = await _paymentService.GenerateOnboardingLinkAsync(
                    acompanante.StripeAccountId,
                    $"{Request.Scheme}://{Request.Host}/api/MetodosPago/onboarding-complete/{acompananteId}?returnUrl={Uri.EscapeDataString(returnUrl)}",
                    $"{Request.Scheme}://{Request.Host}/api/MetodosPago/onboarding-refresh/{acompananteId}?returnUrl={Uri.EscapeDataString(returnUrl)}"
                );

                // Redirigir al usuario al nuevo link de onboarding
                return Redirect(onboardingUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en refresh de onboarding para acompañante {AcompananteId}", acompananteId);
                return StatusCode(500, new { mensaje = "Error al refrescar el proceso de onboarding" });
            }
        }

        [HttpGet("estado/{acompananteId}")]
        [Authorize(Policy = "AcompananteOwnerOrAgencia")]
        public async Task<ActionResult> ObtenerEstadoCuenta(int acompananteId)
        {
            try
            {
                var acompanante = await _acompananteService.GetByIdAsync(acompananteId);
                if (acompanante == null)
                    return NotFound();

                // Verificar permisos
                var usuarioId = ObtenerUsuarioId();
                bool tienePermiso = acompanante.UsuarioId == usuarioId;

                if (!tienePermiso && acompanante.AgenciaId.HasValue)
                {
                    var agencia = await _agenciaService.GetByIdAsync(acompanante.AgenciaId.Value);
                    tienePermiso = agencia != null && agencia.UsuarioId == usuarioId;
                }

                if (!tienePermiso)
                    return Unauthorized();

                // Si no tiene cuenta Stripe, devolver estado básico
                if (string.IsNullOrEmpty(acompanante.StripeAccountId))
                    return Ok(new { estado = "no_account" });

                // Obtener estado actual de la cuenta
                var status = await _paymentService.GetConnectedAccountStatusAsync(acompanante.StripeAccountId);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de cuenta para acompañante {AcompananteId}", acompananteId);
                return StatusCode(500, new { mensaje = "Error al obtener estado de cuenta" });
            }
        }

        [HttpGet("historial-pagos/{acompananteId}")]
        [Authorize(Policy = "AcompananteOwnerOrAgencia")]
        public async Task<ActionResult> ObtenerHistorialPagos(
    int acompananteId,
    [FromQuery] DateTime? desde = null,
    [FromQuery] DateTime? hasta = null,
    [FromQuery] int pagina = 1,
    [FromQuery] int elementosPorPagina = 10)
        {
            try
            {
                var acompanante = await _acompananteService.GetByIdAsync(acompananteId);
                if (acompanante == null)
                    return NotFound();

                // Verificar permisos
                var usuarioId = ObtenerUsuarioId();
                bool tienePermiso = acompanante.UsuarioId == usuarioId;

                if (!tienePermiso && acompanante.AgenciaId.HasValue)
                {
                    var agencia = await _agenciaService.GetByIdAsync(acompanante.AgenciaId.Value);
                    tienePermiso = agencia != null && agencia.UsuarioId == usuarioId;
                }

                if (!tienePermiso)
                    return Unauthorized();

                // Obtener historial de pagos
                var historial = await _paymentService.GetHistorialPagosAsync(
                    acompananteId, desde, hasta, pagina, elementosPorPagina);

                return Ok(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de pagos para acompañante {AcompananteId}", acompananteId);
                return StatusCode(500, new { mensaje = "Error al obtener historial de pagos" });
            }
        }

        [HttpGet("balance/{acompananteId}")]
        [Authorize(Policy = "AcompananteOwnerOrAgencia")]
        public async Task<ActionResult> ObtenerBalance(int acompananteId)
        {
            try
            {
                var acompanante = await _acompananteService.GetByIdAsync(acompananteId);
                if (acompanante == null)
                    return NotFound();

                // Verificar permisos
                var usuarioId = ObtenerUsuarioId();
                if (acompanante.UsuarioId != usuarioId)
                {
                    // Si no es el dueño directo del perfil, verificar si es dueño de la agencia
                    if (acompanante.AgenciaId.HasValue)
                    {
                        var agencia = await _agenciaService.GetByIdAsync(acompanante.AgenciaId.Value);
                        if (agencia == null || agencia.UsuarioId != usuarioId)
                            return Unauthorized();
                    }
                    else
                    {
                        // No es dueño del perfil ni hay agencia asociada
                        return Unauthorized();
                    }
                }

                // Si no tiene cuenta Stripe, devolver balance cero
                if (string.IsNullOrEmpty(acompanante.StripeAccountId))
                    return Ok(new { balance = 0, moneda = "USD" });

                // Obtener balance actual
                var balance = await _paymentService.GetBalanceAsync(acompanante.StripeAccountId);

                return Ok(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener balance para acompañante {AcompananteId}", acompananteId);
                return StatusCode(500, new { mensaje = "Error al obtener balance" });
            }
        }

        private int ObtenerUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("No se pudo identificar al usuario.");

            return userId;
        }

        private async Task<bool> TienePermisoSobreAcompananteAsync(AcompananteDto acompanante, int usuarioId)
        {
            if (acompanante.UsuarioId == usuarioId)
                return true;

            if (acompanante.AgenciaId.HasValue)
            {
                var esOwnerAgencia = await _agenciaService.EsPropietarioAgenciaAsync(
                    acompanante.AgenciaId.Value, usuarioId);
                return esOwnerAgencia;
            }

            return false;
        }


    }
}
