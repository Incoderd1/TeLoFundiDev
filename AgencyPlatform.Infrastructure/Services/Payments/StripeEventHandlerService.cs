using AgencyPlatform.Application.DTOs.Payments;
using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AgencyPlatform.Infrastructure.Services.Payments
{
    public class StripeEventHandlerService : IStripeEventHandlerService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly ISuscripcionVipRepository _suscripcionRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<StripeEventHandlerService> _logger;

        public StripeEventHandlerService(
            ICompraRepository compraRepository,
            ISuscripcionVipRepository suscripcionRepository,
            IClienteRepository clienteRepository,
            ILogger<StripeEventHandlerService> logger)
        {
            _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
            _suscripcionRepository = suscripcionRepository ?? throw new ArgumentNullException(nameof(suscripcionRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //revisar aqui
        public async Task HandleAsync(string eventType, object data)
        {
            switch (eventType)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded((PaymentIntent)data);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed((PaymentIntent)data);  // Nuevo caso para pagos fallidos
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionCanceled((Subscription)data);
                    break;

                // Agrega más casos según lo que quieras manejar
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

                // Lógica para actualizar la compra
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

                // Lógica para actualizar el estado de la compra
                var compra = await _compraRepository.GetByReferenciaAsync(paymentIntent.Id);
                if (compra != null && compra.estado != "fallido")
                {
                    compra.estado = "fallido";  // Actualizamos el estado de la compra a "fallido"
                    await _compraRepository.UpdateAsync(compra);
                    await _compraRepository.SaveChangesAsync();
                    _logger.LogInformation("Compra {CompraId} actualizada a fallida.", compra.id);
                }

                // Aquí también podrías enviar una notificación al cliente o tomar otras acciones
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar el evento 'payment_intent.payment_failed'");
            }


        }





        private async Task HandleSubscriptionCanceled(Subscription subscription)
        {
            try
            {
                _logger.LogInformation("Suscripción cancelada: {SubscriptionId}", subscription.Id);

                // Lógica para cancelar suscripciones
                var suscripcion = await _suscripcionRepository.GetByReferenciaAsync(subscription.Id);
                if (suscripcion != null)
                {
                    suscripcion.estado = "cancelada";  // O cualquier otro estado que desees
                    await _suscripcionRepository.UpdateAsync(suscripcion);
                    await _suscripcionRepository.SaveChangesAsync();
                    _logger.LogInformation("Suscripción {SuscripcionId} cancelada correctamente.", suscripcion.id);
                }

                // Lógica adicional para manejar clientes, membresías VIP, etc.
                //var cliente = await _clienteRepository.GetByIdAsync(subscription.CustomerId);
                //if (cliente != null)
                //{
                //    cliente.es_vip = false; // Si la suscripción VIP es cancelada, desmarcar al cliente como VIP
                //    await _clienteRepository.UpdateAsync(cliente);
                //    await _clienteRepository.SaveChangesAsync();
                //    _logger.LogInformation("Cliente {ClienteId} actualizado como no VIP.", cliente.id);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar el evento 'customer.subscription.deleted'");
            }
        }
    }





}



