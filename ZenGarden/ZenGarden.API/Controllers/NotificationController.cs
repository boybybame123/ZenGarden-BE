using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;
using ZenGarden.API.Services;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController(
    IHubContext<NotificationHub> hubContext,
    RealtimeBackgroundService realtimeBackgroundService) : ControllerBase
{
    [HttpGet("send")]
    public async Task<IActionResult> SendMessage()
    {
        await hubContext.Clients.All.SendAsync("ReceiveMessage", "Hello từ API Server!");
        return Ok("Đã gửi realtime");
    }

    [HttpGet("send-to-user/{userId}")]
    public async Task<IActionResult> SendMessageToUser(string userId)
    {
        await realtimeBackgroundService.NotifyUserOnLogin(userId);
        return Ok("Đã gửi realtime đến user");
    }
}