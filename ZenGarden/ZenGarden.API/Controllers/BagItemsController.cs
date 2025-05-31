using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BagItemsController(IBagItemService bagItemService) : ControllerBase
{
    private readonly IBagItemService _bagItemService =
        bagItemService ?? throw new ArgumentNullException(nameof(bagItemService));

    [HttpGet("{bagId:int}")]
    public async Task<IActionResult> GetListBagItemByBagId(int bagId)
    {
        var bag = await _bagItemService.GetListItemsByBagIdAsync(bagId);
        if (bag == null) return NotFound();
        return Ok(bag);
    }
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetListBagItemByUserId(int userId)
    {
        var bag = await _bagItemService.GetListItemsByUserIdAsync(userId);
        if (bag == null) return NotFound();
        return Ok(bag);
    }
}