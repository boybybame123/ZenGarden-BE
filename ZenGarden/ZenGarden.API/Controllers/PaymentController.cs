using Microsoft.AspNetCore.Mvc;
using Stripe;
using ZenGarden.Core.Services;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(PaymentService paymentService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            var clientSecret = await paymentService.CreatePayment(request);
            return Ok(new { clientSecret });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success(string paymentIntentId)
    {
        await paymentService.HandlePaymentSucceeded(paymentIntentId);
        return Ok("success");
      
    }



    [HttpGet("payment-intent/{paymentIntentId}")]
    public async Task<IActionResult> GetPaymentIntent(string paymentIntentId)
    {
        try
        {
            var paymentIntent = await paymentService.GetStripePaymentInfoAsync(paymentIntentId);
            return Ok(paymentIntent);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("Cancel-payment-intent /{paymentIntentId}")]
    public async Task<IActionResult> CancelPaymentIntent(string paymentIntentId)
    {
        try
        {
           var i = await paymentService.CancelPaymentIntentAsync(paymentIntentId);
            return Ok(i);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


}