using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserXpLogService
{
    Task<UserXpLogDto?> GetUserCheckInLogAsync(int userId, DateTime date);
    Task<double> CheckInAndGetXpAsync(int userId);
}