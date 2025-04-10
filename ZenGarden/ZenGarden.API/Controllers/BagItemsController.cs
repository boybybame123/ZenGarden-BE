using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BagItemsController(IBagItemService bagitemService) : ControllerBase
{
    private readonly IBagItemService _bagitemService = bagitemService ?? throw new ArgumentNullException(nameof(bagitemService));

    [HttpGet("{bagId:int}")]
    public async Task<IActionResult> GetListBagItemByBagId(int bagId)
    {
        var bag = await _bagitemService.GetListItemsByBagIdAsync(bagId);
        if (bag == null) return NotFound();
        return Ok(bag);
    }
}