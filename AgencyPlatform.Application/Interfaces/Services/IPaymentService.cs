using AgencyPlatform.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntent(decimal amount, string currency, string description);
        Task<bool> ConfirmPayment(string paymentIntentId);
        Task<string> CreateSubscription(int clienteId, int membresiaId, string paymentMethodId);
        Task<bool> CancelSubscription(string subscriptionId);




    }

}

