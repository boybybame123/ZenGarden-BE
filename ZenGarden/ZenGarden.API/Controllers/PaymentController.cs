using Microsoft.AspNetCore.Mvc;
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
        return Ok("https://zengarden-fe.vercel.app/home");
    }

    [HttpGet("cancel")]
    public async Task<IActionResult> Cancel(string paymentIntentId)
    {
        await paymentService.HandlePaymentCanceled(paymentIntentId);
        return Ok("https://zengarden-fe.vercel.app/home");
    }
}