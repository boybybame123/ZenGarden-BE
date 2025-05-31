using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface INotificationService
{
    Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);
    Task PushNotificationAsync(int userId, string title, string content);
    Task PushNotificationToAllAsync(string title, string content);
}