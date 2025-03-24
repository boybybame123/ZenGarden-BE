using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TradeTreeController(ITradeTreeService tradeTreeService) : ControllerBase
{
    [HttpPost("trade")]
    public async Task<IActionResult> Trade([FromBody] TradeDto tradeDto)
    {
        var trade = await tradeTreeService.CreateTradeRequestAsync(tradeDto);
        return Ok(trade);
    }

    [HttpPut("accept")]
    public async Task<IActionResult> AcceptTrade(int tradeid, int userbid, int usertreeid)
    {
        var trade = await tradeTreeService.AcceptTradeAsync(tradeid, userbid, usertreeid);
        return Ok(trade);
    }
}