using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Enums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemController : ControllerBase
{
    private readonly IItemDetailService _itemDetailService;
    private readonly IItemService _itemService;
    private readonly IS3Service _s3Service;

    public ItemController(IItemService itemService, IItemDetailService itemDetail, IS3Service s3Service)
    {
        _itemDetailService = itemDetail ?? throw new ArgumentNullException(nameof(itemDetail));
        _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
        _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        try
        {
            var items = await _itemService.GetAllItemsAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving items", error = ex.Message });
        }
    }

    [HttpGet("{itemId:int}")]
    public async Task<IActionResult> GetItemById(int itemId)
    {
        if (itemId <= 0)
            return BadRequest(new { message = "Invalid item ID" });

        try
        {
            var item = await _itemService.GetItemByIdAsync(itemId);
            if (item == null) return NotFound(new { message = $"Item with ID {itemId} not found" });
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving item", error = ex.Message });
        }
    }

    [HttpPut("{itemId:int}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteItem(int itemId)
    {
        if (itemId <= 0)
            return BadRequest(new { message = "Invalid item ID" });

        try
        {
            await _itemService.DeleteItemAsync(itemId);
            return Ok(new { message = "Item deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting item", error = ex.Message });
        }
    }

    [HttpPut("active-item/{itemId:int}")]
    [Produces("application/json")]
    public async Task<IActionResult> ActiveItem(int itemId)
    {
        if (itemId <= 0)
            return BadRequest(new { message = "Invalid item ID" });

        try
        {
            await _itemService.ActiveItem(itemId);
            return Ok(new { message = "Item activated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error activating item", error = ex.Message });
        }
    }

    [HttpPut("update-item/{itemId:int}")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateItem(int itemId, [FromForm] UpdateItemDto item)
    {
        if (itemId <= 0)
            return BadRequest(new { message = "Invalid item ID" });

        if (item == null)
            return BadRequest(new { message = "Item data is required" });

        try
        {
            item.ItemId = itemId;
            var updatedItem = await _itemService.UpdateItemAsync(item);
            return Ok(updatedItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating item", error = ex.Message });
        }
    }

    [HttpPut("update-item-detail/{itemId:int}")]
    public async Task<IActionResult> UpdateItemDetail(int itemId, [FromForm] UpdateItemDetailDto itemDetail)
    {
        if (itemId <= 0)
            return BadRequest(new { message = "Invalid item ID" });

        if (itemDetail == null)
            return BadRequest(new { message = "Item detail data is required" });

        try
        {
            itemDetail.ItemId = itemId;
            var detail = await _itemDetailService.UpdateItemDetailAsync(itemDetail);
            return Ok(detail);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating item detail", error = ex.Message });
        }
    }

    [HttpPost("create-item")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateItem([FromForm] CreateItemDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Request data is required" });

        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Item name is required" });

        if (request.Cost == null || request.Cost <= 0)
            return BadRequest(new { message = "Valid cost is required" });

        if (string.IsNullOrWhiteSpace(request.Rarity))
            return BadRequest(new { message = "Item rarity is required" });

        if (request.File == null)
            return BadRequest(new { message = "Item image is required" });

        if (request.ItemDetail == null)
            return BadRequest(new { message = "Item details are required" });

        // Validate item details
        if (string.IsNullOrWhiteSpace(request.ItemDetail.Description))
            return BadRequest(new { message = "Item description is required" });

        if (!Enum.IsDefined(typeof(ItemType), request.Type))
            return BadRequest(new { message = "Invalid item type" });

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new { message = "Invalid file type. Allowed types: jpg, jpeg, png, gif" });

        // Validate file size (max 5MB)
        if (request.File.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds 5MB limit" });

        try
        {
            // Validate item type specific requirements
            if (request.Type == ItemType.XpBoostTree)
            {
                if (request.ItemDetail.Duration == null || request.ItemDetail.Duration <= 0)
                    return BadRequest(new { message = "XpBoostTree item must have a positive Duration" });

                if (string.IsNullOrWhiteSpace(request.ItemDetail.Effect) ||
                    !int.TryParse(request.ItemDetail.Effect, out var effectValue) ||
                    effectValue < 1 || effectValue > 100)
                    return BadRequest(new { message = "XpBoostTree item Effect must be a number between 1 and 100" });
            }

            var type = _itemDetailService.GetFolderNameByItemType(request.Type);
            var mediaUrl = await _s3Service.UploadFileToFolderAsync(request.File, type);

            if (string.IsNullOrEmpty(mediaUrl))
                return BadRequest(new { message = "Failed to upload file" });

            request.ItemDetail.MediaUrl = mediaUrl;
            var createdItem = await _itemService.CreateItemAsync(request);

            return Ok(new { 
                message = "Item created successfully", 
                data = createdItem,
                mediaUrl 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating item", error = ex.Message });
        }
    }
}