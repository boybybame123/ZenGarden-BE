using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserXpLogService
{
    Task<List<DateTime>> GetUserCheckInHistoryAsync(int userId, int month, int year);
    Task<double> CheckInAndGetXpAsync(int userId);
}