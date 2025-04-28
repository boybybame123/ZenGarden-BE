using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TradeTreeController(ITradeTreeService tradeTreeService,IUserTreeService userTreeService) : ControllerBase
{
    [HttpPost("trade")]
    public async Task<IActionResult> Trade([FromBody] TradeDto tradeDto)
    {
        var trade = await tradeTreeService.CreateTradeRequestAsync(tradeDto);
        return Ok(trade);
    }

    [HttpPut("accept")]
    public async Task<IActionResult> AcceptTrade(int tradeId, int userId, int userTreeId)
    {
        var trade = await tradeTreeService.AcceptTradeAsync(tradeId, userId, userTreeId);
        return Ok(trade);
    }

    [HttpGet("history/by-status/{status}")]
    public async Task<IActionResult> GetTradeHistoryByStatus(int status)
    {
        var tradeHistory = await tradeTreeService.GetTradeHistoryByStatusAsync(status);
       
        return Ok(tradeHistory);
    }

    [HttpGet("history/by-id/{tradeId}")]
    public async Task<IActionResult> GetTradeHistoryById(int tradeId)
    {
        var tradeHistory = await tradeTreeService.GetTradeHistoryByIdAsync(tradeId);
        return Ok(tradeHistory);
    }

    [HttpDelete("cancel/{tradeId}")]
    public async Task<IActionResult> CancelTrade(int tradeId, int userA)
    {
        var result = await tradeTreeService.CancelTradeAsync(tradeId, userA);
        return Ok(result);
    }

    [HttpGet("history/all/{userId}")]
    public async Task<IActionResult> GetAllTradeHistoriesByOwnerId(int userId)
    {
        var tradeHistory = await tradeTreeService.GetAllTradeHistoriesByOwneridAsync(userId);
        return Ok(tradeHistory);
    }

    [HttpGet("history/not-owner/{userId}")]
    public async Task<IActionResult> GetAllTradeHistoriesByNotOwnerId(int userId)
    {
        var tradeHistory = await tradeTreeService.GetAllTradeHistoriesByNotOwnerIdAsync(userId);
        return Ok(tradeHistory);
    }
}