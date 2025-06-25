using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZenGarden.Core.Hubs;
using ZenGarden.Core.Interfaces.IServices;


namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController(
    IHubContext<NotificationHub> hubContext,INotificationService notificationService) : ControllerBase
{
    [HttpGet("send")]
    public async Task<IActionResult> SendMessage()
    {
        await hubContext.Clients.All.SendAsync("ReceiveMessage", "Hello từ API Server!");
        return Ok("Đã gửi realtime");
    }

    [HttpGet("user/{userId}/notifications")]
    public async Task<IActionResult> GetNotificationsByUserId(string userId)
    {
        var notifications = await notificationService.GetNotificationsByUserIdAsync(int.Parse(userId));

        return Ok(notifications);
    }
}