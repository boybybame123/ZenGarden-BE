using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TradeHistoriesController(ITradeHistoryService tradeHistoryService) : ControllerBase
{
    private readonly ITradeHistoryService _tradehistoryService = tradeHistoryService ?? throw new ArgumentNullException(nameof(tradeHistoryService));

    [HttpGet]
    public async Task<IActionResult> Gettradehistorys()
    {
        var tradehistorys = await _tradehistoryService.GetTradeHistoryAsync();
        return Ok(tradehistorys);
    }

}