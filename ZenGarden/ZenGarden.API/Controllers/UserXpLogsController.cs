using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserXpLogsController(IUserXpLogService userXpLogService)
    : ControllerBase
{
    [HttpPost("confirm-focus/{userId:int}")]
    public async Task<IActionResult> GetUserCheckInLog(int userId)
    {
        var today = DateTime.UtcNow.Date;
        var log = await userXpLogService.GetUserCheckInLogAsync(userId, today);

        if (log == null)
            return NotFound(new { message = "User has not checked in today." });

        return Ok(log);
    }

    [HttpPost("claim-daily-xp/{userId:int}")]
    public async Task<IActionResult> CheckInUser(int userId)
    {
        var xpEarned = await userXpLogService.CheckInAndGetXpAsync(userId);
        return Ok(new { message = "Check-in successful!", xpEarned });
    }
}