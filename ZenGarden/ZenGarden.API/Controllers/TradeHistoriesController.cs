using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TradeHistoriesController(ITradeHistoryService tradeHistoryService) : ControllerBase
{
    private readonly ITradeHistoryService _tradeHistoryService =
        tradeHistoryService ?? throw new ArgumentNullException(nameof(tradeHistoryService));

    [HttpGet]
    public async Task<IActionResult> GetTradeHistory()
    {
        var tradeHistory = await _tradeHistoryService.GetTradeHistoryAsync();
        return Ok(tradeHistory);
    }
}