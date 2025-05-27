using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class FocusActivityRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<FocusActivity>(context, redisService), IFocusActivityRepository
{
    private readonly ZenGardenContext _context = context;
    private readonly IRedisService _redisService = redisService;

    public async Task<List<FocusActivity>> GetByTrackingIdAsync(int trackingId)
    {
        var cacheKey = $"FocusActivity:tracking:{trackingId}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await _context.FocusActivity
                .Where(a => a.TrackingId == trackingId)
                .OrderBy(a => a.Timestamp)
                .ToListAsync(),
            TimeSpan.FromMinutes(30)
        );
    }

    public async Task<int> GetTotalDistractionDurationAsync(int trackingId)
    {
        var cacheKey = $"FocusActivity:distraction:{trackingId}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await _context.FocusActivity
                .Where(a => a.TrackingId == trackingId && a.IsDistraction)
                .SumAsync(a => a.Duration ?? 0),
            TimeSpan.FromMinutes(30)
        );
    }
}