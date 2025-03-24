using Microsoft.AspNetCore.SignalR;

namespace ZenGarden.API.Hubs;

public class NotificationHub : Hub
{
    // Ví dụ hàm đơn giản nhận tin nhắn từ client (có thể mở rộng)
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}