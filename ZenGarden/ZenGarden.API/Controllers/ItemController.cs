using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemController : ControllerBase
{
    private readonly IItemService itemService;

    public ItemController(IItemService itemService)
    {
        this.itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
    }

    // GET: api/item
    [HttpGet]
    public async Task<IActionResult> Getitems()
    {
        var items = await itemService.GetAllItemsAsync();
        return Ok(items);
    }

    [HttpGet("{itemId}")]
    public async Task<IActionResult> GetitemById(int itemId)
    {
        var item = await itemService.GetItemByIdAsync(itemId);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }
    [HttpDelete("{itemId}")]
    [Produces("application/json")]
    public async Task<IActionResult> Deleteitem(int itemId)
    {
        await itemService.DeleteItemAsync(itemId);
        return Ok(new { message = "item deleted successfully" });
    }

    [HttpPut("update-item")]
    [Produces("application/json")]
    public async Task<IActionResult> Updateitem(ItemDto item)
    {
        await itemService.UpdateItemAsync(item);
        return Ok(new { message = "item updated successfully" });
    }
}
