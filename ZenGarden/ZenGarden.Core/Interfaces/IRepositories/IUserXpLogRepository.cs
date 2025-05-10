using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserXpLogRepository : IGenericRepository<UserXpLog>
{
    Task<UserXpLog?> GetUserCheckInLogAsync(int userId, DateTime date);
    Task<UserXpLog?> GetLastCheckInLogAsync(int userId);
    Task<UserXpLog?> GetLastXpLogAsync(int userId);
    Task<List<UserXpLog>> GetUserCheckInsByMonthAsync(int userId, int month, int year);
    Task<List<UserXpLog>> GetAllUserXpLogsAsync();
    Task<List<UserXpLog>> GetUserXpLogsByUserIdAsync(int userId);
}