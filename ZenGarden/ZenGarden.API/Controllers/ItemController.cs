using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemController(IItemService itemService) : ControllerBase
{
    private readonly IItemService _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var items = await _itemService.GetAllItemsAsync();
        return Ok(items);
    }

    [HttpGet("{itemId}")]
    public async Task<IActionResult> GetItemById(int itemId)
    {
        var item = await _itemService.GetItemByIdAsync(itemId);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("{itemId}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteItem(int itemId)
    {
        await _itemService.DeleteItemAsync(itemId);
        return Ok(new { message = "item deleted successfully" });
    }

    [HttpPut("update-item")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateItem(ItemDto item)
    {
        await _itemService.UpdateItemAsync(item);
        return Ok(new { message = "item updated successfully" });
    }
}