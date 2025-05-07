using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Repositories;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgencyPlatform.Application.DTOs.Payment;
using AgencyPlatform.Application.DTOs.MetodoPago;

namespace AgencyPlatform.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly ITransaccionRepository _transaccionRepository;
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IAgenciaRepository _agenciaRepository;
        private readonly decimal _comisionAgencia = 0.20m; // 20% de comisión

        public StripePaymentService(
            IConfiguration configuration,
            ILogger<StripePaymentService> logger,
            ITransaccionRepository transaccionRepository,
            ITransferenciaRepository transferenciaRepository,
            IAcompananteRepository acompananteRepository,
            IAgenciaRepository agenciaRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _transaccionRepository = transaccionRepository;
            _transferenciaRepository = transferenciaRepository;
            _acompananteRepository = acompananteRepository;
            _agenciaRepository = agenciaRepository;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            _logger.LogInformation("StripePaymentService inicializado con API Key: {KeyFirstChar}***",
                _configuration["Stripe:SecretKey"]?.Substring(0, 1) ?? "No configurada");
        }

        public async Task<string> CreatePaymentIntent(decimal amount, string currency, string description, Dictionary<string, string> metadata = null)
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
                    Metadata = metadata,
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
                    Brand = pm.Card?.Brand,
                    Last4 = pm.Card?.Last4,
                    ExpiryMonth = pm.Card?.ExpMonth ?? 0,
                    ExpiryYear = pm.Card?.ExpYear ?? 0
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

        public async Task<string> CreateCheckoutSession(string productName, int amount, string referenceId, string successUrl, string cancelUrl)
        {
            try
            {
                _logger.LogInformation("Creando sesión de checkout: Producto={ProductName}, Monto={Amount}, ReferenceId={ReferenceId}",
                    productName, amount, referenceId);

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = amount,
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = productName,
                                },
                            },
                            Quantity = 1,
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "referenceId", referenceId }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation("Sesión de pago creada: ID={SessionId}", session.Id);
                return session.Url;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al crear sesión de pago: {ErrorMessage}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear sesión de pago");
                throw;
            }
        }

        public async Task<transaccion> ProcesarPagoCliente(int clienteId, int acompananteId, decimal monto, string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Procesando pago de cliente ID: {ClienteId} para acompañante ID: {AcompananteId}. Monto: {Monto}", clienteId, acompananteId, monto);

                var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
                if (acompanante == null)
                {
                    _logger.LogWarning("Acompañante ID: {AcompananteId} no encontrado", acompananteId);
                    throw new InvalidOperationException("Acompañante no encontrado");
                }

                // Crear el PaymentIntent
                var metadata = new Dictionary<string, string>
                {
                    { "clienteId", clienteId.ToString() },
                    { "acompananteId", acompananteId.ToString() },
                    { "agenciaId", acompanante.agencia_id?.ToString() ?? "N/A" }
                };

                var clientSecret = await CreatePaymentIntent(
                    amount: monto,
                    currency: "usd",
                    description: $"Pago por servicio de acompañante ID: {acompananteId}",
                    metadata: metadata
                );

                // Confirmar el pago
                var paymentIntentId = clientSecret.Split("_secret_")[0];
                var pagoConfirmado = await ConfirmPayment(paymentIntentId);

                if (!pagoConfirmado)
                {
                    _logger.LogWarning("Pago fallido para cliente ID: {ClienteId}. PaymentIntentId: {PaymentIntentId}", clienteId, paymentIntentId);
                    throw new InvalidOperationException("El pago no pudo ser procesado");
                }

                // Crear la transacción
                var transaccion = new transaccion
                {
                    cliente_id = clienteId,
                    acompanante_id = acompananteId,
                    agencia_id = acompanante.agencia_id,
                    monto_total = monto,
                    monto_acompanante = acompanante.agencia_id.HasValue ? monto * (1 - _comisionAgencia) : monto,
                    monto_agencia = acompanante.agencia_id.HasValue ? monto * _comisionAgencia : null,
                    estado = "pendiente",
                    proveedor_pago = "stripe",
                    id_transaccion_externa = paymentIntentId,
                    fecha_transaccion = DateTime.UtcNow,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                await _transaccionRepository.AddAsync(transaccion);
                await _transaccionRepository.SaveChangesAsync();

                _logger.LogInformation("Pago procesado con éxito. Transacción ID: {TransaccionId}", transaccion.id);

                // Distribuir el pago al acompañante
                transaccion = await DistribuirPagoAAcompanante(transaccion);

                return transaccion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago de cliente ID: {ClienteId} para acompañante ID: {AcompananteId}", clienteId, acompananteId);
                throw;
            }
        }

        public async Task<transaccion> DistribuirPagoAAcompanante(transaccion transaccion)
        {
            try
            {
                if (!transaccion.acompanante_id.HasValue)
                {
                    throw new InvalidOperationException("La transacción no tiene un acompañante asignado");
                }

                var acompanante = await _acompananteRepository.GetByIdAsync(transaccion.acompanante_id.Value);
                if (acompanante == null)
                {
                    throw new InvalidOperationException("Acompañante no encontrado");
                }

                if (transaccion.agencia_id.HasValue)
                {
                    var transferenciaAgencia = new transferencia
                    {
                        transaccion_id = transaccion.id,
                        origen_id = transaccion.cliente_id,
                        origen_tipo = "cliente",
                        destino_id = transaccion.agencia_id.Value,
                        destino_tipo = "agencia",
                        monto = transaccion.monto_agencia ?? 0,
                        estado = "pendiente",
                        proveedor_pago = "stripe",
                        fecha_creacion = DateTime.UtcNow,
                        created_at = DateTime.UtcNow,
                        updated_at = DateTime.UtcNow
                    };

                    await _transferenciaRepository.AddAsync(transferenciaAgencia);
                    await _transferenciaRepository.SaveChangesAsync();

                    // Simular la transferencia
                    transferenciaAgencia.id_transferencia_externa = "simulated_transfer_" + transferenciaAgencia.id;
                    transferenciaAgencia.estado = "completado";
                    transferenciaAgencia.fecha_procesamiento = DateTime.UtcNow;
                    await _transferenciaRepository.UpdateAsync(transferenciaAgencia);
                    await _transferenciaRepository.SaveChangesAsync();
                }

                var transferenciaAcompanante = new transferencia
                {
                    transaccion_id = transaccion.id,
                    origen_id = transaccion.agencia_id ?? transaccion.cliente_id,
                    origen_tipo = transaccion.agencia_id.HasValue ? "agencia" : "cliente",
                    destino_id = transaccion.acompanante_id.Value,
                    destino_tipo = "acompanante",
                    monto = transaccion.monto_acompanante,
                    estado = "pendiente",
                    proveedor_pago = "stripe",
                    fecha_creacion = DateTime.UtcNow,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                await _transferenciaRepository.AddAsync(transferenciaAcompanante);
                await _transferenciaRepository.SaveChangesAsync();

                // Simular la transferencia
                transferenciaAcompanante.id_transferencia_externa = "simulated_transfer_" + transferenciaAcompanante.id;
                transferenciaAcompanante.estado = "completado";
                transferenciaAcompanante.fecha_procesamiento = DateTime.UtcNow;
                await _transferenciaRepository.UpdateAsync(transferenciaAcompanante);
                await _transferenciaRepository.SaveChangesAsync();

                transaccion.estado = "completado";
                transaccion.fecha_procesamiento = DateTime.UtcNow;
                transaccion.updated_at = DateTime.UtcNow;
                await _transaccionRepository.UpdateAsync(transaccion);
                await _transaccionRepository.SaveChangesAsync();

                return transaccion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al distribuir pago");
                transaccion.estado = "fallido";
                transaccion.updated_at = DateTime.UtcNow;
                await _transaccionRepository.UpdateAsync(transaccion);
                await _transaccionRepository.SaveChangesAsync();
                throw;
            }
        }

        public async Task<transaccion> DistribuirPagoAAgencia(int agenciaId, decimal monto, string paymentIntentId)
        {
            try
            {
                _logger.LogInformation("Distribuyendo pago a la agencia ID: {AgenciaId}. Monto: {Monto}, PaymentIntentId: {PaymentIntentId}", agenciaId, monto, paymentIntentId);

                // Obtener la agencia
                var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
                if (agencia == null)
                {
                    _logger.LogWarning("Agencia ID: {AgenciaId} no encontrada", agenciaId);
                    throw new InvalidOperationException("Agencia no encontrada");
                }

                // Verificar si la agencia tiene una cuenta conectada en Stripe
                string stripeAccountId = agencia.stripe_account_id;
                if (string.IsNullOrEmpty(stripeAccountId))
                {
                    _logger.LogInformation("La agencia ID: {AgenciaId} no tiene una cuenta conectada en Stripe, creando una...", agenciaId);
                    stripeAccountId = await CreateConnectedAccountAsync(
                        email: agencia.email ?? throw new InvalidOperationException("La agencia debe tener un email registrado"),
                        type: "express",
                        metadata: new Dictionary<string, string>
                        {
                            { "agenciaId", agenciaId.ToString() }
                        }
                    );

                    // Actualizar la agencia con el nuevo stripe_account_id
                    agencia.stripe_account_id = stripeAccountId;
                    await _agenciaRepository.UpdateAsync(agencia);
                    await _agenciaRepository.SaveChangesAsync();
                    _logger.LogInformation("Cuenta conectada creada para agencia ID: {AgenciaId}. Stripe Account ID: {StripeAccountId}", agenciaId, stripeAccountId);
                }

                // Crear una transacción para registrar el pago
                var transaccion = new transaccion
                {
                    agencia_id = agenciaId,
                    monto_total = monto,
                    monto_agencia = monto, // Todo el monto va a la agencia
                    estado = "pendiente",
                    proveedor_pago = "stripe",
                    id_transaccion_externa = paymentIntentId,
                    fecha_transaccion = DateTime.UtcNow,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                await _transaccionRepository.AddAsync(transaccion);
                await _transaccionRepository.SaveChangesAsync();

                // Crear una transferencia a la cuenta conectada de la agencia usando Stripe Connect
                var transferenciaAgencia = new transferencia
                {
                    transaccion_id = transaccion.id,
                    origen_id = 0, // Plataforma como origen
                    origen_tipo = "plataforma",
                    destino_id = agenciaId,
                    destino_tipo = "agencia",
                    monto = monto,
                    estado = "pendiente",
                    proveedor_pago = "stripe",
                    fecha_creacion = DateTime.UtcNow,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                await _transferenciaRepository.AddAsync(transferenciaAgencia);
                await _transferenciaRepository.SaveChangesAsync();

                // Crear la transferencia real con Stripe Connect
                var transferOptions = new TransferCreateOptions
                {
                    Amount = (long)(monto * 100), // Stripe usa centavos
                    Currency = "usd",
                    Destination = stripeAccountId,
                    TransferGroup = $"verificacion_agencia_{agenciaId}_transaccion_{transaccion.id}",
                    SourceTransaction = paymentIntentId // Vincular la transferencia al PaymentIntent
                };

                var transferService = new TransferService();
                var transfer = await transferService.CreateAsync(transferOptions);

                // Actualizar la transferencia con los datos reales
                transferenciaAgencia.id_transferencia_externa = transfer.Id;
                transferenciaAgencia.estado = "completado";
                transferenciaAgencia.fecha_procesamiento = DateTime.UtcNow;
                await _transferenciaRepository.UpdateAsync(transferenciaAgencia);
                await _transferenciaRepository.SaveChangesAsync();

                // Actualizar la transacción
                transaccion.estado = "completado";
                transaccion.fecha_procesamiento = DateTime.UtcNow;
                transaccion.updated_at = DateTime.UtcNow;
                await _transaccionRepository.UpdateAsync(transaccion);
                await _transaccionRepository.SaveChangesAsync();

                _logger.LogInformation("Pago distribuido a la agencia ID: {AgenciaId}. Transacción ID: {TransaccionId}, Transferencia ID: {TransferId}", agenciaId, transaccion.id, transfer.Id);
                return transaccion;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al distribuir pago a la agencia ID: {AgenciaId}: {ErrorMessage}, Código: {ErrorCode}", agenciaId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al distribuir pago a la agencia ID: {AgenciaId}", agenciaId);
                throw;
            }
        }

        public async Task<decimal> ObtenerSaldoAcompanante(int acompananteId)
        {
            try
            {
                _logger.LogInformation("Obteniendo saldo para acompañante ID: {AcompananteId}", acompananteId);
                var transferencias = await _transferenciaRepository.GetByAcompananteIdAsync(acompananteId);
                var saldo = transferencias
                    .Where(t => t.estado == "completado" && t.destino_tipo == "acompanante")
                    .Sum(t => t.monto);
                _logger.LogInformation("Saldo para acompañante ID: {AcompananteId} es {Saldo}", acompananteId, saldo);
                return saldo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener saldo para acompañante ID: {AcompananteId}", acompananteId);
                throw;
            }
        }

        public async Task<string> CreateConnectedAccountAsync(string email, string type, Dictionary<string, string> metadata = null)
        {
            try
            {
                _logger.LogInformation("Creando cuenta conectada de Stripe: Email={Email}, Tipo={Type}", email, type);

                var options = new AccountCreateOptions
                {
                    Type = type,
                    Email = email,
                    Capabilities = new AccountCapabilitiesOptions
                    {
                        Transfers = new AccountCapabilitiesTransfersOptions
                        {
                            Requested = true,
                        },
                        CardPayments = new AccountCapabilitiesCardPaymentsOptions
                        {
                            Requested = true,
                        },
                    },
                    BusinessType = type == "express" ? "individual" : "company",
                    Metadata = metadata
                };

                var service = new AccountService();
                var account = await service.CreateAsync(options);

                _logger.LogInformation("Cuenta conectada creada: AccountId={AccountId}", account.Id);
                return account.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error al crear cuenta conectada de Stripe");
                throw;
            }
        }

        public async Task<string> GenerateOnboardingLinkAsync(string stripeAccountId, string returnUrl, string refreshUrl)
        {
            try
            {
                _logger.LogInformation("Generando enlace de onboarding para Stripe Account ID: {StripeAccountId}", stripeAccountId);

                var options = new AccountLinkCreateOptions
                {
                    Account = stripeAccountId,
                    Type = "account_onboarding",
                    ReturnUrl = returnUrl,
                    RefreshUrl = refreshUrl
                };

                var service = new AccountLinkService();
                var accountLink = await service.CreateAsync(options);

                _logger.LogInformation("Enlace de onboarding generado para Stripe Account ID: {StripeAccountId}. URL: {AccountLinkUrl}", stripeAccountId, accountLink.Url);
                return accountLink.Url;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al generar enlace de onboarding para Stripe Account ID: {StripeAccountId}: {ErrorMessage}, Código: {ErrorCode}", stripeAccountId, ex.Message, ex.StripeError?.Code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al generar enlace de onboarding para Stripe Account ID: {StripeAccountId}", stripeAccountId);
                throw;
            }
        }


        public async Task<StripeAccountStatusDto> GetConnectedAccountStatusAsync(string stripeAccountId)
        {
            try
            {
                var service = new AccountService();
                var account = await service.GetAsync(stripeAccountId);

                return new StripeAccountStatusDto
                {
                    Status = account.BusinessType,
                    PayoutsEnabled = account.PayoutsEnabled,
                    ChargesEnabled = account.ChargesEnabled,
                    AccountId = account.Id,
                    RequiereAccion = !(account.DetailsSubmitted && account.PayoutsEnabled)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de cuenta conectada {StripeAccountId}", stripeAccountId);
                throw;
            }
        }

        public async Task<bool> ActualizarEstadoCuentaConectadaAsync(string stripeAccountId)
        {
            try
            {
                var service = new AccountService();
                var account = await service.GetAsync(stripeAccountId);

                // Busca el acompañante asociado a esta cuenta
                var acompanante = await _acompananteRepository.GetByStripeAccountIdAsync(stripeAccountId);
                if (acompanante == null)
                {
                    _logger.LogWarning("No se encontró acompañante con cuenta Stripe {StripeAccountId}", stripeAccountId);
                    return false;
                }

                // Actualiza el estado en la base de datos
                acompanante.stripe_payouts_enabled = account.PayoutsEnabled;
                acompanante.stripe_charges_enabled = account.ChargesEnabled;
                acompanante.stripe_onboarding_completed = account.DetailsSubmitted;

                await _acompananteRepository.UpdateAsync(acompanante);
                await _acompananteRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de cuenta conectada {StripeAccountId}", stripeAccountId);
                return false;
            }
        }

        public async Task<List<TransaccionPagoDto>> GetHistorialPagosAsync(
            int acompananteId,
            DateTime? desde = null,
            DateTime? hasta = null,
            int pagina = 1,
            int elementosPorPagina = 10)
        {
            try
            {
                var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
                if (acompanante == null || string.IsNullOrEmpty(acompanante.stripe_account_id))
                    return new List<TransaccionPagoDto>();

                // Obtener pagos de la base de datos
                var transacciones = await _transaccionRepository.GetByAcompananteIdAsync(
                    acompananteId, desde, hasta, pagina, elementosPorPagina);

                return transacciones.Select(t => new TransaccionPagoDto
                {
                    Id = t.id_transaccion_externa ?? t.id.ToString(),
                    Monto = t.monto_acompanante,
                    Moneda = "USD", // O usar la moneda de la transacción si está disponible
                    Estado = t.estado,
                    FechaCreacion = t.fecha_transaccion,
                    Concepto = t.concepto
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de pagos para acompañante {AcompananteId}", acompananteId);
                return new List<TransaccionPagoDto>();
            }
        }
        public async Task<BalanceDto> GetBalanceAsync(string stripeAccountId)
        {
            try
            {
                var service = new BalanceService();
                var balance = await service.GetAsync(null, new RequestOptions { StripeAccount = stripeAccountId });

                // Obtener el balance disponible en USD (o la moneda principal)
                var disponible = balance.Available.FirstOrDefault(b => b.Currency == "usd") ?? balance.Available.FirstOrDefault();
                var pendiente = balance.Pending.FirstOrDefault(b => b.Currency == "usd") ?? balance.Pending.FirstOrDefault();

                return new BalanceDto
                {
                    Disponible = disponible != null ? Convert.ToDecimal(disponible.Amount) / 100 : 0,
                    Pendiente = pendiente != null ? Convert.ToDecimal(pendiente.Amount) / 100 : 0,
                    Moneda = disponible?.Currency?.ToUpper() ?? "USD"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener balance para cuenta Stripe {StripeAccountId}", stripeAccountId);
                return new BalanceDto { Disponible = 0, Pendiente = 0, Moneda = "USD" };
            }
        }



        // En StripePaymentService
        public async Task<bool> TransferirAAcompanante(int acompananteId, decimal monto, string descripcion)
        {
            try
            {
                var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
                if (acompanante == null || string.IsNullOrEmpty(acompanante.stripe_account_id))
                    throw new Exception($"Acompañante {acompananteId} no tiene cuenta Stripe configurada");

                // Transferir dinero a la cuenta Stripe Connect del acompañante
                var transferOptions = new TransferCreateOptions
                {
                    Amount = (long)(monto * 100),
                    Currency = "usd",
                    Destination = acompanante.stripe_account_id,
                    Description = descripcion
                };

                var transferService = new TransferService();
                var transfer = await transferService.CreateAsync(transferOptions);

                // Registrar la transferencia en nuestra base de datos
                var transferencia = new transferencia
                {
                    origen_tipo = "plataforma",
                    destino_id = acompananteId,
                    destino_tipo = "acompanante",
                    monto = monto,
                    concepto = descripcion,
                    estado = "completado",
                    proveedor_pago = "stripe",
                    id_transferencia_externa = transfer.Id,
                    fecha_procesamiento = DateTime.UtcNow
                };

                await _transferenciaRepository.AddAsync(transferencia);
                await _transferenciaRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al transferir dinero a acompañante {AcompananteId}", acompananteId);
                return false;
            }
        }

    }
}