using AgencyPlatform.Application.DTOs.Payments;
using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AgencyPlatform.Infrastructure.Services.Payments
{
    public class StripeEventHandlerService : IStripeEventHandlerService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly ISuscripcionVipRepository _suscripcionRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IAgenciaService _agenciaService; // Nueva dependencia
        private readonly ILogger<StripeEventHandlerService> _logger;

        public StripeEventHandlerService(
            ICompraRepository compraRepository,
            ISuscripcionVipRepository suscripcionRepository,
            IClienteRepository clienteRepository,
            IAgenciaService agenciaService,
            ILogger<StripeEventHandlerService> logger)
        {
            _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
            _suscripcionRepository = suscripcionRepository ?? throw new ArgumentNullException(nameof(suscripcionRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _agenciaService = agenciaService ?? throw new ArgumentNullException(nameof(agenciaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(string eventType, object data)
        {
            switch (eventType)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded((PaymentIntent)data);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed((PaymentIntent)data);
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionCanceled((Subscription)data);
                    break;

                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted((Stripe.Checkout.Session)data);
                    break;

                default:
                    _logger.LogWarning($"Evento desconocido: {eventType}");
                    break;
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
        {
            try
            {
                _logger.LogInformation("Pago completado para PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

                var compra = await _compraRepository.GetByReferenciaAsync(paymentIntent.Id);
                if (compra != null && compra.estado != "completado")
                {
                    compra.estado = "completado";
                    await _compraRepository.UpdateAsync(compra);
                    await _compraRepository.SaveChangesAsync();
                    _logger.LogInformation("Compra {CompraId} actualizada a completada.", compra.id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar el evento 'payment_intent.succeeded'");
            }
        }

        private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
        {
            try
            {
                _logger.LogInformation("Pago fallido para PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

                var compra = await _compraRepository.GetByReferenciaAsync(paymentIntent.Id);
                if (compra != null && compra.estado != "fallido")
                {
                    compra.estado = "fallido";
                    await _compraRepository.UpdateAsync(compra);
                    await _compraRepository.SaveChangesAsync();
                    _logger.LogInformation("Compra {CompraId} actualizada a fallida.", compra.id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar el evento 'payment_intent.payment_failed'");
            }
        }

        private async Task HandleCheckoutSessionCompleted(Stripe.Checkout.Session session)
        {
            try
            {
                _logger.LogInformation("Sesión de checkout completada: {SessionId}", session.Id);

                if (session.Metadata.TryGetValue("referenceId", out var referenceId))
                {
                    var anuncio = await _agenciaService.GetAnuncioByReferenceIdAsync(referenceId);
                    if (anuncio != null)
                    {
                        anuncio.EstaActivo = true;
                        await _agenciaService.UpdateAnuncioAsync(anuncio);
                        _logger.LogInformation("Anuncio {AnuncioId} activado tras confirmar pago con referencia {ReferenceId}", anuncio.Id, referenceId);
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró anuncio con referencia {ReferenceId}", referenceId);
                    }
                }
                else
                {
                    _logger.LogWarning("No se encontró referenceId en la sesión de checkout: {SessionId}", session.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar el evento 'checkout.session.completed'");
            }
        }

        private async Task HandleSubscriptionCanceled(Subscription subscription)
        {
            try
            {
                _logger.LogInformation("Suscripción cancelada: {SubscriptionId}", subscription.Id);

                var suscripcion = await _suscripcionRepository.GetByReferenciaAsync(subscription.Id);
                if (suscripcion != null)
                {
                    suscripcion.estado = "cancelada";
                    await _suscripcionRepository.UpdateAsync(suscripcion);
                    await _suscripcionRepository.SaveChangesAsync();
                    _logger.LogInformation("Suscripción {SuscripcionId} cancelada correctamente.", suscripcion.id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar el evento 'customer.subscription.deleted'");
            }
        }
    }
}