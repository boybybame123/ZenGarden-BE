using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserXpConfigRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<UserXpConfig>(context, redisService), IUserXpConfigRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<int> GetXpThresholdForLevel(int levelId)
    {
        return await _context.UserXpConfig
            .Where(x => x.LevelId == levelId)
            .Select(x => x.XpThreshold)
            .FirstOrDefaultAsync();
    }

    public async Task<UserXpConfig> GetMaxLevelConfig()
    {
        return await _context.UserXpConfig
            .OrderByDescending(x => x.LevelId)
            .FirstOrDefaultAsync() ?? throw new InvalidOperationException("Max level config not found");
    }
}