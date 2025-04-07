using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UseItemController(IUseItemService useItemService) : ControllerBase
{
    [HttpPost("use")]
    public async Task<IActionResult> UseItem(int itemBagId, int? userTreeId)

    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var parsedUserId)) return BadRequest(new { message = "Invalid user ID" });

        var result = await useItemService.UseItemAsync(parsedUserId, itemBagId, userTreeId);

        return Ok(result);
    }
}