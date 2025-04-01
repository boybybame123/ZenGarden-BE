using Microsoft.AspNetCore.Mvc;
using Stripe.Forwarding;
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


    [HttpPut("update-item-detail")]
    public async Task<IActionResult> UpdateItemDetail([FromForm]UpdateItemDetailDto itemDetail)
    {
        



        await _itemDetailService.UpdateItemDetailAsync(itemDetail);
        return Ok(new { message = "item detail updated successfully" });
    }


    [HttpPost("create-item")]
    public async Task<IActionResult> UploadAndCreateItem([FromForm] CreateItemDto request)
    {
        // Validate the uploaded file
        if (request.File == null)
            return BadRequest(new { message = "File is required" });

        // Upload file to S3
        var mediaUrl =
            await _s3Service.UploadFileAsync(request
                .File); // Null forgiving operator is safe here due to the earlier null check

        if (string.IsNullOrEmpty(mediaUrl))
            return BadRequest(new { message = "File upload failed" });

        // Ensure ItemDetail is not null and assign MediaUrl
        if (request.ItemDetail == null)
            return BadRequest(new { message = "ItemDetail is required." });

        request.ItemDetail.MediaUrl = mediaUrl;

        // Save item to database (any exceptions here will be handled by middleware)
        await _itemService.CreateItemAsync(request);

        // Return successful response
        return Ok(new { message = "Item created successfully", mediaUrl });
    }
}