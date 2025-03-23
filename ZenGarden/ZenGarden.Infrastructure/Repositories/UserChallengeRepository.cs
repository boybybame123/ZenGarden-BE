using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserChallengeRepository(ZenGardenContext context)
    : GenericRepository<UserChallenge>(context), IUserChallengeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId)
    {
        return await _context.UserChallenges
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);
    }

    public async Task<List<UserChallenge>> GetAllUsersInChallengeAsync(int challengeId)
    {
        return await _context.UserChallenges
            .Where(uc => uc.ChallengeId == challengeId)
            .ToListAsync();
    }
}