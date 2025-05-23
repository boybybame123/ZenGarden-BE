using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserConfigRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<UserConfig>(context, redisService), IUserConfigRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<UserConfig?> GetUserConfigbyUserIdAsync(int userId)
    {
        return await _context.UserConfig.FindAsync(userId);
    }
}