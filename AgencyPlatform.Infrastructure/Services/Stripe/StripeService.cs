using AgencyPlatform.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Stripe
{
    public class StripeService : IPaymentService
    {
        private readonly string _apiKey;

        public StripeService(IConfiguration configuration)
        {
            _apiKey = configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = _apiKey;
        }

        public async Task<string> CreatePaymentIntent(decimal amount, string currency, string description)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe usa centavos
                Currency = currency,
                Description = description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return intent.ClientSecret;
        }

        public async Task<bool> ConfirmPayment(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(paymentIntentId);

            // Verificar el estado del pago
            return intent.Status == "succeeded";
        }

        public async Task<string> CreateSubscription(int clienteId, int membresiaId, string paymentMethodId)
        {
            // Primero, crear o recuperar un cliente en Stripe
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Description = $"Cliente ID: {clienteId}",
                PaymentMethod = paymentMethodId,
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethodId,
                }
            });

            // Crear la suscripción
            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
            {
                Customer = customer.Id,
                Items = new List<SubscriptionItemOptions>
        {
            new SubscriptionItemOptions
            {
                Price = $"price_membresia_{membresiaId}", // ID del precio en Stripe
            },
        },
            });

            return subscription.Id;
        }

        public async Task<bool> CancelSubscription(string subscriptionId)
        {
            var service = new SubscriptionService();
            var subscription = await service.CancelAsync(subscriptionId, null);

            return subscription.Status == "canceled";
        }

    }
}
