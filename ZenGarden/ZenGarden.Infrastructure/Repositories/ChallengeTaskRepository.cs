using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ChallengeTaskRepository(ZenGardenContext context)
    : GenericRepository<ChallengeTask>(context), IChallengeTaskRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<ChallengeTask>> GetTasksByChallengeIdAsync(int challengeId)
    {
        return await _context.ChallengeTask.Where(ct => ct.ChallengeId == challengeId).ToListAsync();
    }
}