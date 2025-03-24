using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemController(IItemService itemService, IItemDetailService itemDetail, IS3Service s3Service)
    : ControllerBase
{
    private readonly IItemDetailService _itemDetailService =
        itemDetail ?? throw new ArgumentNullException(nameof(itemDetail));

    private readonly IItemService _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));

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

    [HttpPut("{itemId}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteItem(int itemId)
    {
        await _itemService.DeleteItemAsync(itemId);
        return Ok(new { message = "item deleted successfully" });
    }


    [HttpPut("active-item/{itemId}")]
    [Produces("application/json")]
    public async Task<IActionResult> ActiveItem(int itemId)
    {
        await _itemService.ActiveItem(itemId);
        return Ok(new { message = "item activated successfully" });
    }


    [HttpPut("update-item")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateItem(UpdateItemDto item)
    {
        await _itemService.UpdateItemAsync(item);
        return Ok(new { message = "item updated successfully" });
    }


    [HttpPut("update-itemdetail")]
    public async Task<IActionResult> UpdateItemDetail(UpdateItemDetailDto itemDetail)
    {
        await _itemDetailService.UpdateItemDetailAsync(itemDetail);
        return Ok(new { message = "item detail updated successfully" });
    }


    [HttpPost("create-item")]
    public async Task<IActionResult> UploadAndCreateItem([FromForm] ItemDto request)
    {
        // Upload file lên S3
        var mediaUrl = await _s3Service.UploadFileAsync(request.File);


        // Convert ItemJson => ItemDto

        // Gán mediaUrl
        request.ItemDetail.MediaUrl = mediaUrl;

        // Lưu DB
        await _itemService.CreateItemAsync(request);

        return Ok(new { message = "Item created successfully", mediaUrl });
    }
}