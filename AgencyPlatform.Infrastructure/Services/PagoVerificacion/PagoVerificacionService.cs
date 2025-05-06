using AgencyPlatform.Application.DTOs.Verificaciones;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.PagoVerificacion;
using AgencyPlatform.Infrastructure.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AgencyPlatform.Infrastructure.Services.PagoVerificacion
{
    public class PagoVerificacionService : IPagoVerificacionService
    {
        private readonly IPagoVerificacionRepository _pagoVerificacionRepository;
        private readonly IAgenciaRepository _agenciaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<PagoVerificacionService> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IPaymentService _paymentService;
        private readonly IAcompananteRepository _compananteRepository;
        private readonly IConfiguration _configuration;

        public PagoVerificacionService(
            IPagoVerificacionRepository pagoVerificacionRepository,
            IAgenciaRepository agenciaRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            ILogger<PagoVerificacionService> logger,
            IEmailSender emailSender,
            IPaymentService paymentService,
            IConfiguration configuration)
        {
            _pagoVerificacionRepository = pagoVerificacionRepository;
            _agenciaRepository = agenciaRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _logger = logger;
            _emailSender = emailSender;
            _paymentService = paymentService;
            _configuration = configuration;
        }

        public async Task<List<PagoVerificacionDto>> GetPagosByAgenciaIdAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);
            var pagos = await _pagoVerificacionRepository.GetByAgenciaIdAsync(agenciaId);
            return _mapper.Map<List<PagoVerificacionDto>>(pagos);
        }

        public async Task<List<PagoVerificacionDto>> GetPagosPendientesByAgenciaIdAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);
            var pagos = await _pagoVerificacionRepository.GetPendientesByAgenciaIdAsync(agenciaId);
            return _mapper.Map<List<PagoVerificacionDto>>(pagos);
        }

        public async Task<PagoVerificacionDto> GetPagoByIdAsync(int pagoId)
        {
            var pago = await _pagoVerificacionRepository.GetByIdAsync(pagoId);
            if (pago == null)
                throw new Exception("Pago no encontrado");

            await VerificarPermisosAgencia(pago.agencia_id);
            return _mapper.Map<PagoVerificacionDto>(pago);
        }

        public async Task ConfirmarPagoAsync(int pagoId, string referenciaPago)
        {
            var pago = await _pagoVerificacionRepository.GetByIdAsync(pagoId);
            if (pago == null)
                throw new Exception("Pago no encontrado");

            await VerificarPermisosAgencia(pago.agencia_id);

            if (pago.estado != "pendiente")
                throw new Exception("Este pago ya ha sido procesado");

            pago.estado = "completado";
            pago.fecha_pago = DateTime.UtcNow;
            pago.referencia_pago = referenciaPago;

            await _pagoVerificacionRepository.UpdateAsync(pago);
            await _pagoVerificacionRepository.SaveChangesAsync();

            // Enviar notificación de pago completado
            try
            {
                if (pago.acompanante?.usuario?.email != null)
                {
                    await _emailSender.SendEmailAsync(
                        pago.acompanante.usuario.email,
                        "Verificación completada",
                        $"Tu verificación ha sido completada exitosamente. Referencia de pago: {referenciaPago}"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de pago completado");
            }
        }

        public async Task CancelarPagoAsync(int pagoId, string motivo)
        {
            var pago = await _pagoVerificacionRepository.GetByIdAsync(pagoId);
            if (pago == null)
                throw new Exception("Pago no encontrado");

            await VerificarPermisosAgencia(pago.agencia_id);

            if (pago.estado != "pendiente")
                throw new Exception("Este pago ya ha sido procesado");

            pago.estado = "cancelado";
            pago.referencia_pago = motivo;

            await _pagoVerificacionRepository.UpdateAsync(pago);
            await _pagoVerificacionRepository.SaveChangesAsync();

            // Enviar notificación de pago cancelado
            try
            {
                if (pago.acompanante?.usuario?.email != null)
                {
                    await _emailSender.SendEmailAsync(
                        pago.acompanante.usuario.email,
                        "Pago de verificación cancelado",
                        $"El pago para tu verificación ha sido cancelado. Motivo: {motivo}"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de pago cancelado");
            }
        }
        public async Task<string> CrearSesionPagoAsync(int verificacionId)
        {
            var pago = await _pagoVerificacionRepository.GetByVerificacionIdAsync(verificacionId);
            if (pago == null)
                throw new Exception("Pago no encontrado");

            await VerificarPermisosAgencia(pago.agencia_id);

            if (pago.estado != "pendiente")
                throw new Exception("Este pago ya ha sido procesado");

            var acompanante = await _compananteRepository.GetByIdAsync(pago.acompanante_id);
            var nombreAcompanante = acompanante?.nombre_perfil ?? "Acompañante";

            var productoNombre = $"Verificación de {nombreAcompanante}";
            var urlBase = _configuration["ApplicationUrl"];
            var successUrl = $"{urlBase}/pagos/verificacion/success?id={pago.id}";
            var cancelUrl = $"{urlBase}/pagos/verificacion/cancel?id={pago.id}";

            return null;
        }

        private async Task VerificarPermisosAgencia(int agenciaId)
        {
            if (EsAdmin())
                return; // Los administradores tienen acceso a todas las agencias

            var usuarioId = ObtenerUsuarioId();
            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId);

            if (agencia == null || agencia.id != agenciaId)
                throw new UnauthorizedAccessException("No tienes permisos para acceder a esta agencia.");
        }

        private int ObtenerUsuarioId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                throw new UnauthorizedAccessException("No se pudo identificar al usuario.");

            return userId;
        }

        private bool EsAdmin()
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole("admin") ?? false;
        }
        public async Task<string> CrearIntentoPagoAsync(int verificacionId)
        {
            var pago = await _pagoVerificacionRepository.GetByVerificacionIdAsync(verificacionId);
            if (pago == null)
                throw new Exception("Pago no encontrado");

            await VerificarPermisosAgencia(pago.agencia_id);

            if (pago.estado != "pendiente")
                throw new Exception("Este pago ya ha sido procesado");

            var acompanante = await _compananteRepository.GetByIdAsync(pago.acompanante_id);
            var nombreAcompanante = acompanante?.nombre_perfil ?? "Acompañante";

            // Crear intento de pago con metadata
            var metadata = new Dictionary<string, string>
    {
        { "tipo", "verificacion" },
        { "verificacionId", verificacionId.ToString() },
        { "acompananteId", pago.acompanante_id.ToString() },
        { "agenciaId", pago.agencia_id.ToString() }
    };

            var clientSecret = await _paymentService.CreatePaymentIntent(
        pago.monto,
        pago.moneda ?? "usd",
        $"Verificación de {nombreAcompanante}",
        metadata  // Asegúrate de que metadata es Dictionary<string, string>
    );

            return clientSecret;
        }

      

    }
}
