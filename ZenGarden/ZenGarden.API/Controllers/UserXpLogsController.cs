using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.API.Middleware;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserXpLogsController(IUserXpLogService userXpLogService)
    : ControllerBase
{
    [Authorize]
    [HttpGet("checkin-history")]
    public async Task<IActionResult> GetUserCheckInHistory([FromQuery] int? month, [FromQuery] int? year)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized(new { error = "Invalid or missing user ID." });

        var currentDate = DateTime.UtcNow;
        var targetMonth = month ?? currentDate.Month;
        var targetYear = year ?? currentDate.Year;

        var history = await userXpLogService.GetUserCheckInHistoryAsync(userId, targetMonth, targetYear);
        return Ok(history);
    }


    [Authorize]
    [HttpPost("claim-daily-xp")]
    public async Task<IActionResult> CheckInUser()
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var (xp, message) = await userXpLogService.CheckInAndGetXpAsync(userId.Value);
        return Ok(new
        {
            Success = xp > 0,
            XpEarned = xp,
            Message = message
        });
    }

    [Authorize]
    [HttpGet("streak")]
    public async Task<IActionResult> GetCurrentStreak()
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var streak = await userXpLogService.GetCurrentStreakAsync(userId.Value);
        return Ok(new { streak });
    }

    [HttpPost("check-level-up/{userId:int}")]
    public async Task<IActionResult> CheckLevelUp(int userId)
    {
        await userXpLogService.CheckLevelUpAsync(userId);
        return Ok(new { message = "Level check and update completed successfully." });
    }
    
    [HttpGet]
    public async Task<ActionResult<List<UserXpLogDto>>> GetAllUserXpLogs()
    {
        var logs = await userXpLogService.GetAllUserXpLogsAsync();
        return Ok(logs);
    }
    
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<List<UserXpLogDto>>> GetUserXpLogsByUserId(int userId)
    {
        var logs = await userXpLogService.GetUserXpLogsByUserIdAsync(userId);
        return Ok(logs);
    }
}