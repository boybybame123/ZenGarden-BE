using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Services;

public class NotificationService(
    ZenGardenContext db,
    IHubContext<NotificationHub> hubContext,
    ILogger<NotificationService> logger)
    : INotificationService
{
    public async Task PushNotificationAsync(int userId, string title, string content)
    {
        var notification = new Notification
        {
            UserId = userId,
            Content = $"{title}: {content}",
            CreatedAt = DateTime.UtcNow
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        logger.LogInformation($"✅ Notification saved and pushed to user {userId}");

        // Gửi đến đúng user qua NotificationHub
        await hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", notification.Content, notification.CreatedAt);

        logger.LogInformation("📢 [SignalR] Sent to user {UserId}: {NotificationContent}", userId,
            notification.Content);
    }
}