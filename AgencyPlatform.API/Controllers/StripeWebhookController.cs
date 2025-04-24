using System;
using System.IO;
using System.Threading.Tasks;
using AgencyPlatform.Application.DTOs.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/stripe/webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripeEventHandlerService _eventHandlerService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IStripeEventHandlerService eventHandlerService,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger)
        {
            _eventHandlerService = eventHandlerService ?? throw new ArgumentNullException(nameof(eventHandlerService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            // 1) Leer el body completo
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // 2) Construir y validar el evento de Stripe
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"]
                );
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Firma de webhook Stripe inválida.");
                return BadRequest();
            }

            // 3) Extraer el tipo de evento
            var eventType = stripeEvent.Type;

            // 4) Delegar el manejo al servicio
            try
            {
                await _eventHandlerService.HandleAsync(eventType, stripeEvent.Data.Object);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el evento Stripe: {EventType}", eventType);
                return StatusCode(500);
            }
        }
    }
}
