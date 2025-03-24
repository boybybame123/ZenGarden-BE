using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationController(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpGet("send")]
    public async Task<IActionResult> SendMessage()
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Hello từ API Server!");
        return Ok("Đã gửi realtime");
    }
}
