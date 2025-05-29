using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FocusMethodsController(IFocusMethodService focusMethodService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllFocusMethods()
    {
        var focusMethods = await focusMethodService.GetAllFocusMethodsAsync();
        return Ok(focusMethods);
    }

    [HttpPost("suggest")]
    public async Task<IActionResult> SuggestFocusMethod([FromBody] SuggestFocusMethodDto dto)
    {
        var suggestedMethod = await focusMethodService.SuggestFocusMethodAsync(dto);
        return Ok(suggestedMethod);
    }
}