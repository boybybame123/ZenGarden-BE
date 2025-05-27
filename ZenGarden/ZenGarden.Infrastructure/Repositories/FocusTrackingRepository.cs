using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class FocusTrackingRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<FocusTracking>(context, redisService), IFocusTrackingRepository
{
    private readonly ZenGardenContext _context = context;
    private readonly IRedisService _redisService = redisService;

    public async Task<List<FocusTracking>> GetByUserIdAsync(int userId)
    {
        var cacheKey = $"FocusTracking:user:{userId}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await _context.FocusTracking
                .Include(t => t.FocusActivities)
                .Include(t => t.FocusMethod)
                .Include(t => t.Task)
                    .ThenInclude(t => t.TaskType)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync(),
            TimeSpan.FromMinutes(30)
        );
    }

    public async Task<FocusTracking?> GetByIdWithDetailsAsync(int id)
    {
        var cacheKey = $"FocusTracking:{id}:with_details";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await _context.FocusTracking
                .Include(t => t.FocusActivities)
                .Include(t => t.FocusMethod)
                .Include(t => t.Task)
                    .ThenInclude(t => t.TaskType)
                .FirstOrDefaultAsync(t => t.TrackingId == id),
            TimeSpan.FromMinutes(30)
        );
    }
}