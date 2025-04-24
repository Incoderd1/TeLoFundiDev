using AgencyPlatform.Application.DTOs.Payments;
using AgencyPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IStripeEventHandlerService _stripeEventHandlerService;
        private readonly IConfiguration _configuration;

        public PaymentsController(
            IPaymentService paymentService,
            IStripeEventHandlerService stripeEventHandlerService,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _stripeEventHandlerService = stripeEventHandlerService;
            _configuration = configuration;
        }

        [HttpPost("create-payment-intent")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDto dto)
        {
            try
            {
                var clientSecret = await _paymentService.CreatePaymentIntent(
                    dto.Amount,
                    dto.Currency ?? "usd",
                    dto.Description ?? "Compra en la plataforma"
                );
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear la intención de pago", error = ex.Message });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"]
                );

                await _stripeEventHandlerService.HandleAsync(stripeEvent.Type, stripeEvent.Data.Object);
                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}