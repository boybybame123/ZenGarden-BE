using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ChallengeRepository(ZenGardenContext context)
    : GenericRepository<Challenge>(context), IChallengeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<Challenge>> GetChallengeAll()
    {
        return await _context.Challenge
            .Include(u => u.ChallengeType)
            .Include(u => u.UserChallenges)
            .Include(u => u.ChallengeTasks)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<Challenge?> GetByIdChallengeAsync(int id)
    {
        return await _context.Challenge
            .Include(u => u.ChallengeType)
            .Include(u => u.UserChallenges)
            .Include(u => u.ChallengeTasks)
            .Where(fm => fm.ChallengeId == id)
            .FirstOrDefaultAsync();
    }
}