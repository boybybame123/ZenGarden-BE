using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ZenGarden.API.Hubs;

public class NotificationHub : Hub
{
    // Ví dụ hàm đơn giản nhận tin nhắn từ client (có thể mở rộng)
    private static readonly ConcurrentDictionary<string, string> Connections = new();

    public override Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return base.OnConnectedAsync();
        Connections[userId] = Context.ConnectionId;
        Console.WriteLine($"✅ User {userId} connected with {Context.ConnectionId}");

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Connections.ContainsKey(userId)) return base.OnDisconnectedAsync(exception);
        Connections.TryRemove(userId, out _);
        Console.WriteLine($"❌ User {userId} disconnected");

        return base.OnDisconnectedAsync(exception);
    }

    // Hàm gửi tin nhắn tới một user cụ thể
    public async Task SendNotificationToUser(string userId, string message)
    {
        if (Connections.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveNotification", message);
            Console.WriteLine($"📢 Sent notification to user {userId}: {message}");
        }
    }
}