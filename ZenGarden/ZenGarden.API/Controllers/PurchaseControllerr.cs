using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    /// <summary>
    ///     API mua item
    /// </summary>
    /// <param name="request">Thông tin mua item</param>
    /// <returns>Thông báo kết quả mua</returns>
    [HttpPost("buy")]
    public async Task<IActionResult> BuyItem([FromBody] PurchaseRequest request)
    {


        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);



        var result = await _purchaseService.PurchaseItem(userId, request.ItemId);
        if (result == "Purchase successful.")
            return Ok(new { message = result });

        return BadRequest(new { message = result });
    }
}