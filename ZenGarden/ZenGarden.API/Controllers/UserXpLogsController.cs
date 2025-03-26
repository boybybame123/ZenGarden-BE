using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

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
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized(new { error = "Invalid or missing user ID." });

        var xpEarned = await userXpLogService.CheckInAndGetXpAsync(userId);
        if (xpEarned == 0)
            return BadRequest(new { message = "Already checked in today." });

        return Ok(new { message = "Check-in successful!", xpEarned });
    }
}