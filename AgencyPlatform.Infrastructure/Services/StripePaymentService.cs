using AgencyPlatform.Application.DTOs.Payment;
using AgencyPlatform.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            _logger.LogInformation("StripePaymentService inicializado con API Key: {KeyFirstChar}***",
                _configuration["Stripe:SecretKey"]?.Substring(0, 1) ?? "No configurada");
        }

        public async Task<string> CreatePaymentIntent(decimal amount, string currency, string description)
        {
            try
            {
                _logger.LogInformation("Creando PaymentIntent: Monto={Amount}, Moneda={Currency}, Descripción={Description}",
                    amount, currency, description);

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

                _logger.LogInformation("PaymentIntent creado: ID={PaymentIntentId}", intent.Id);
                return intent.ClientSecret;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al crear PaymentIntent: {ErrorMessage}, Código: {ErrorCode}",
                    ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear PaymentIntent");
                throw;
            }
        }

        public async Task<bool> ConfirmPayment(string paymentIntentId)
        {
            try
            {
                _logger.LogInformation("Confirmando pago para PaymentIntent: {PaymentIntentId}", paymentIntentId);

                var service = new PaymentIntentService();
                var intent = await service.GetAsync(paymentIntentId);

                _logger.LogInformation("Estado del PaymentIntent {PaymentIntentId}: {Status}",
                    paymentIntentId, intent.Status);

                return intent.Status == "succeeded";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al confirmar pago {PaymentIntentId}: {ErrorMessage}, Código: {ErrorCode}",
                    paymentIntentId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al confirmar pago {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        public async Task<string> CreateSubscription(int clienteId, int membresiaId, string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Creando suscripción: ClienteId={ClienteId}, MembresiaId={MembresiaId}",
                    clienteId, membresiaId);

                // Crear o recuperar un cliente en Stripe
                var customerService = new CustomerService();
                _logger.LogDebug("Creando/recuperando cliente en Stripe para ClienteId={ClienteId}", clienteId);

                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Description = $"Cliente ID: {clienteId}",
                    PaymentMethod = paymentMethodId,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId,
                    }
                });

                _logger.LogDebug("Cliente creado en Stripe: {CustomerId}", customer.Id);

                // Crear la suscripción
                var priceId = $"price_membresia_{membresiaId}";
                _logger.LogDebug("Usando price_id={PriceId} para la suscripción", priceId);

                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
                {
                    Customer = customer.Id,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = priceId,
                        },
                    },
                });

                _logger.LogInformation("Suscripción creada: ID={SubscriptionId}, Estado={Status}",
                    subscription.Id, subscription.Status);

                return subscription.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al crear suscripción para ClienteId={ClienteId}: {ErrorMessage}, Código: {ErrorCode}",
                    clienteId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear suscripción para ClienteId={ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<bool> CancelSubscription(string subscriptionId)
        {
            try
            {
                _logger.LogInformation("Cancelando suscripción: {SubscriptionId}", subscriptionId);

                var service = new SubscriptionService();
                var subscription = await service.CancelAsync(subscriptionId, null);

                _logger.LogInformation("Suscripción {SubscriptionId} cancelada, estado: {Status}",
                    subscriptionId, subscription.Status);

                return subscription.Status == "canceled";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al cancelar suscripción {SubscriptionId}: {ErrorMessage}, Código: {ErrorCode}",
                    subscriptionId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cancelar suscripción {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<PaymentStatusDto> GetPaymentStatus(string paymentIntentId)
        {
            try
            {
                _logger.LogInformation("Consultando estado del pago: {PaymentIntentId}", paymentIntentId);

                var service = new PaymentIntentService();
                var intent = await service.GetAsync(paymentIntentId);

                _logger.LogDebug("Estado obtenido para {PaymentIntentId}: {Status}", paymentIntentId, intent.Status);

                return new PaymentStatusDto
                {
                    Status = intent.Status,
                    PaymentIntentId = intent.Id,
                    ClientSecret = intent.ClientSecret
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al obtener estado del pago {PaymentIntentId}: {ErrorMessage}, Código: {ErrorCode}",
                    paymentIntentId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener estado del pago {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        public async Task<string> AttachPaymentMethod(string customerId, string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Asociando método de pago: Cliente={CustomerId}, PaymentMethod={PaymentMethodId}",
                    customerId, paymentMethodId);

                var options = new PaymentMethodAttachOptions
                {
                    Customer = customerId,
                };

                var service = new PaymentMethodService();
                var paymentMethod = await service.AttachAsync(paymentMethodId, options);

                _logger.LogInformation("Método de pago asociado correctamente: {PaymentMethodId}", paymentMethod.Id);

                return paymentMethod.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al asociar método de pago {PaymentMethodId} a cliente {CustomerId}: {ErrorMessage}, Código: {ErrorCode}",
                    paymentMethodId, customerId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al asociar método de pago {PaymentMethodId} a cliente {CustomerId}",
                    paymentMethodId, customerId);
                throw;
            }
        }

        public async Task<string> CreateCustomer(int clienteId, string email, string nombre)
        {
            try
            {
                _logger.LogInformation("Creando cliente en Stripe: ClienteId={ClienteId}, Email={Email}",
                    clienteId, email);

                var options = new CustomerCreateOptions
                {
                    Email = email,
                    Name = nombre,
                    Metadata = new Dictionary<string, string>
                    {
                        { "ClienteId", clienteId.ToString() }
                    }
                };

                var service = new CustomerService();
                var customer = await service.CreateAsync(options);

                _logger.LogInformation("Cliente creado en Stripe: {CustomerId}", customer.Id);

                return customer.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al crear cliente para ClienteId={ClienteId}: {ErrorMessage}, Código: {ErrorCode}",
                    clienteId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear cliente para ClienteId={ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<List<PaymentMethodDto>> GetCustomerPaymentMethods(string customerId)
        {
            try
            {
                _logger.LogInformation("Obteniendo métodos de pago para cliente: {CustomerId}", customerId);

                var options = new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = "card"
                };

                var service = new PaymentMethodService();
                var paymentMethods = await service.ListAsync(options);

               

                return paymentMethods.Select(pm => new PaymentMethodDto
                {
                    Id = pm.Id,
                    Type = pm.Type,
                    Brand = pm.Card.Brand,
                    Last4 = pm.Card.Last4,
                    ExpiryMonth = pm.Card.ExpMonth,
                    ExpiryYear = pm.Card.ExpYear
                }).ToList();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al obtener métodos de pago para cliente {CustomerId}: {ErrorMessage}, Código: {ErrorCode}",
                    customerId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener métodos de pago para cliente {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> SetDefaultPaymentMethod(string customerId, string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Estableciendo método de pago predeterminado: Cliente={CustomerId}, PaymentMethod={PaymentMethodId}",
                    customerId, paymentMethodId);

                var options = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };

                var service = new CustomerService();
                var customer = await service.UpdateAsync(customerId, options);

                // Corrigiendo la comparación con la propiedad DefaultPaymentMethod que puede ser un string
                bool isDefault = customer.InvoiceSettings?.DefaultPaymentMethod != null &&
                                 customer.InvoiceSettings.DefaultPaymentMethod.Equals(paymentMethodId);

                _logger.LogInformation("Método de pago predeterminado establecido para cliente {CustomerId}: {Success}",
                    customerId, isDefault ? "Exitoso" : "Fallido");

                return isDefault;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al establecer método de pago predeterminado {PaymentMethodId} para cliente {CustomerId}: {ErrorMessage}, Código: {ErrorCode}",
                    paymentMethodId, customerId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al establecer método de pago predeterminado {PaymentMethodId} para cliente {CustomerId}",
                    paymentMethodId, customerId);
                throw;
            }
        }
    }
}