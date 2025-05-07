using AgencyPlatform.Application.DTOs.MetodoPago;
using AgencyPlatform.Application.DTOs.Payment;
using AgencyPlatform.Core.Entities;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntent(decimal amount, string currency, string description, Dictionary<string, string> metadata = null);
        Task<bool> ConfirmPayment(string paymentIntentId);
        Task<string> CreateSubscription(int clienteId, int membresiaId, string paymentMethodId);
        Task<bool> CancelSubscription(string subscriptionId);
        Task<PaymentStatusDto> GetPaymentStatus(string paymentIntentId);
        Task<string> AttachPaymentMethod(string customerId, string paymentMethodId);
        Task<string> CreateCustomer(int clienteId, string email, string nombre);
        Task<List<PaymentMethodDto>> GetCustomerPaymentMethods(string customerId);
        Task<bool> SetDefaultPaymentMethod(string customerId, string paymentMethodId);
        Task<string> CreateCheckoutSession(string productName, int amount, string referenceId, string successUrl, string cancelUrl);

        Task<transaccion> ProcesarPagoCliente(int clienteId, int acompananteId, decimal monto, string paymentMethodId);
        Task<transaccion> DistribuirPagoAAcompanante(transaccion transaccion);
        Task<decimal> ObtenerSaldoAcompanante(int acompananteId);

        Task<string> CreateConnectedAccountAsync(string email, string type, Dictionary<string, string> metadata = null);



        //Task<transaccion> DistribuirPagoAAgencia(int agenciaId, decimal monto, string paymentIntentId);

        Task<transaccion> DistribuirPagoAAgencia(int agenciaId, decimal monto, string paymentIntentId);
        Task<string> GenerateOnboardingLinkAsync(string stripeAccountId, string returnUrl, string refreshUrl);


       
        Task<StripeAccountStatusDto> GetConnectedAccountStatusAsync(string stripeAccountId);
        Task<bool> ActualizarEstadoCuentaConectadaAsync(string stripeAccountId);
        Task<List<TransaccionPagoDto>> GetHistorialPagosAsync(int acompananteId, DateTime? desde = null, DateTime? hasta = null, int pagina = 1, int elementosPorPagina = 10);
        Task<BalanceDto> GetBalanceAsync(string stripeAccountId);


    }

    

}

