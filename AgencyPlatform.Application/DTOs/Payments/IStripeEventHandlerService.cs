using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Payments
{
    public interface IStripeEventHandlerService
    {
        Task HandleAsync(string eventType, object data);


    }
}
