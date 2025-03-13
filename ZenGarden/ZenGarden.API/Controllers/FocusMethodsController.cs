using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FocusMethodsController(IFocusMethodService focusMethodService) : ControllerBase
{
    [HttpPost("suggest")]
    public async Task<IActionResult> SuggestFocusMethod([FromBody] SuggestFocusMethodDto dto)
    {
        var suggestedMethod = await focusMethodService.SuggestFocusMethodAsync(dto);
        return Ok(suggestedMethod);
    }
}