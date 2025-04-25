namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserXpLogService
{
    Task<List<DateTime>> GetUserCheckInHistoryAsync(int userId, int month, int year);
    Task<(double xpEarned, string notificationMessage)> CheckInAndGetXpAsync(int userId);
    Task<int> GetCurrentStreakAsync(int userId);
    Task<double> AddXpForStartTaskAsync(int userId);
    Task CheckLevelUpAsync(int userId);
}