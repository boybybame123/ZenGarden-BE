using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UseItemController(IUseItemService useItemService) : ControllerBase
{
    [HttpPost("use")]
    public async Task<IActionResult> UseItem(int itemBagId)

    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var parsedUserId)) return BadRequest(new { message = "Invalid user ID" });

        var result = await useItemService.UseItemAsync(parsedUserId, itemBagId);

        return Ok(result);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(int itemBagId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var parsedUserId)) return BadRequest(new { message = "Invalid user ID" });
        await useItemService.Cancel(itemBagId);
        return Ok(new { message = "Item cancelled successfully" });
    }
    [HttpGet("xp-boost-tree-remaining-time")]
    public async Task<IActionResult> GetXpBoostTreeRemainingTime()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var parsedUserId))
            return BadRequest(new { message = "Invalid user ID" });

        var (itemId, remainingSeconds) = await useItemService.GetXpBoostTreeRemainingTimeAsync(parsedUserId);

        return Ok(new { ItemId = itemId, RemainingSeconds = remainingSeconds });
    }
    
}