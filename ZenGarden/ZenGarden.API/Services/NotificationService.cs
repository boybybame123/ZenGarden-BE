using Microsoft.AspNetCore.SignalR;
using System;
using ZenGarden.API.Hubs;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

public class NotificationService : INotificationService
{
    private readonly ZenGardenContext _db;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ZenGardenContext db, IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _db = db;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PushNotificationAsync(int userId, string title, string content)
    {
        var noti = new Notification
        {
            UserId = userId,
            Content = $"{title}: {content}",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(noti);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"✅ Notification saved and pushed to user {userId}");

        // Bắn đúng user
        await _hubContext.Clients.User(userId.ToString())
                         .SendAsync("ReceiveNotification", noti.Content, noti.CreatedAt);
        _logger.LogInformation($"✅ [SignalR] Pushed to user {userId}: {noti.Content}");
    }
}
