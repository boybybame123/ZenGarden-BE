using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BagsController(IBagService bagService) : ControllerBase
{
    private readonly IBagService _bagService = bagService ?? throw new ArgumentNullException(nameof(bagService));

    [HttpGet("{bagId}")]
    public async Task<IActionResult> GetItemById(int bagId)
    {
        var bag = await _bagService.GetBagByIdAsync(bagId);
        if (bag == null) return NotFound();
        return Ok(bag);
    }
}