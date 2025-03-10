using Microsoft.AspNetCore.Mvc;
using Stripe;
using ZenGarden.Core.Services;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(PaymentService paymentService) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var clientSecret = await paymentService.CreatePaymentIntent(request);
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"],
                "whsec_6IjPYRvzTVNalopb8mHYCaXah5e4BSRI");

            if (stripeEvent.Type != "payment_intent.succeeded") return Ok();
            if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
            {
                await paymentService.HandlePaymentSucceeded(paymentIntent.Id);
            }
            return Ok();
        }
    }
}