namespace ZenGarden.Core.Interfaces.IServices;

public interface INotificationService
{
    Task PushNotificationAsync(int userId, string title, string content);
}