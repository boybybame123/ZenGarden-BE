﻿using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemController(IItemService itemService,IItemDetailService itemDetail,IS3Service s3Service) : ControllerBase
{
    private readonly IItemService _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
    private readonly IItemDetailService _itemDetailService = itemDetail ?? throw new ArgumentNullException(nameof(itemDetail));
    private readonly IS3Service _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));


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

    [HttpPost("create-item")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateItem(ItemDto item)
    {
        await _itemService.CreateItemAsync(item);
        return Ok(new { message = "item created successfully" });
    }
}