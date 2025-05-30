using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ChallengeTaskRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<ChallengeTask>(context, redisService), IChallengeTaskRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<ChallengeTask>> GetTasksByChallengeIdAsync(int challengeId)
    {
        return await _context.ChallengeTask
            .Where(ct => ct.ChallengeId == challengeId)
            .Include(ct => ct.Tasks)
            .ToListAsync();
    }

    public async Task<ChallengeTask?> GetByTaskIdAsync(int taskId)
    {
        return await _context.ChallengeTask
            .FirstOrDefaultAsync(ct => ct.TaskId == taskId);
    }

    public async Task<List<ChallengeTask>> GetAllByTaskIdAsync(int taskId)
    {
        return await _context.ChallengeTask
            .Where(ct => ct.TaskId == taskId)
            .ToListAsync();
    }
}