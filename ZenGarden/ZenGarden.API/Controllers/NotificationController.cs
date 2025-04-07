using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController(IHubContext<NotificationHub> hubContext) : ControllerBase
{
    [HttpGet("send")]
    public async Task<IActionResult> SendMessage()
    {
        await hubContext.Clients.All.SendAsync("ReceiveMessage", "Hello từ API Server!");
        return Ok("Đã gửi realtime");
    }
}