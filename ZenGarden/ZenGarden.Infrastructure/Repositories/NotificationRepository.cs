using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class NotificationRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<Notification>(context, redisService), INotificationRepository
{
    // No additional methods needed, inherits from GenericRepository
    public async Task<List<Notification>> GetByUserIdAsync(int userId)
    {
        return await GetListAsync(n => n.UserId == userId || n.UserId == null);
    }
}