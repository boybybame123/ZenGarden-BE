using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.API.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IHubContext<NotificationHub> hubContext,
    ILogger<NotificationService> logger)
    : INotificationService
{
    public async Task PushNotificationAsync(int userId, string title, string content)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = "system",
            CreatedAt = DateTime.UtcNow
        };

        await notificationRepository.CreateAsync(notification);


        logger.LogInformation($"✅ Notification saved and pushed to user {userId}");

        // Gửi đến đúng user qua NotificationHub
        await hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", notification.Title, notification.Content, notification.CreatedAt);

        logger.LogInformation("📢 [SignalR] Sent to user {UserId}: {Title}, {Content}", userId, notification.Title,
            notification.Content);
    }

    public async Task PushNotificationToAllAsync(string title, string content)
    {
        var notification = new Notification
        {
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await notificationRepository.CreateAsync(notification);

        logger.LogInformation("✅ Notification saved and pushed to all users");

        // Gửi đến tất cả user qua NotificationHub
        await hubContext.Clients.All.SendAsync(
            "ReceiveNotification",
            notification.Title,
            notification.Content,
            notification.CreatedAt
        );

        logger.LogInformation("📢 [SignalR] Sent to all users:{Title}, {Content}", notification.Title,
            notification.Content);
    }
}