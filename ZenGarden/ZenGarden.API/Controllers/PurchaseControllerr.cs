using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController(IPurchaseService purchaseService) : ControllerBase
{
    /// <summary>
    ///     API mua item
    /// </summary>
    /// <param name="request">Thông tin mua item</param>
    /// <returns>Thông báo kết quả mua</returns>
    [HttpPost("buy")]
    public async Task<IActionResult> BuyItem([FromBody] PurchaseRequest request)
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);


        var result = await purchaseService.PurchaseItem(userId, request.ItemId);
        if (result == "Purchase successful.")
            return Ok(new { message = result });

        return BadRequest(new { message = result });
    }
}