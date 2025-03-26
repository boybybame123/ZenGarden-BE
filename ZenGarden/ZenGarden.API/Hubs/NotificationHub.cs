using Microsoft.AspNetCore.SignalR;

namespace ZenGarden.API.Hubs;

public class NotificationHub : Hub
{
    // Ví dụ hàm đơn giản nhận tin nhắn từ client (có thể mở rộng)
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
    public async Task SendToUser(string userId, string title, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", title, message);
    }
        public async Task BroadcastNotification(string title, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", title, message);
    }

}