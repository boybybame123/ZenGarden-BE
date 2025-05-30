using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserChallengeRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<UserChallenge>(context, redisService), IUserChallengeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId)
    {
        return await _context.UserChallenges
            .Include(uc => uc.User)
            .Include(uc => uc.Challenge)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);
    }

    public async Task<List<UserChallenge>> GetAllUsersInChallengeAsync(int challengeId)
    {
        return await _context.UserChallenges
            .Where(uc => uc.ChallengeId == challengeId)
            .ToListAsync();
    }

    public async Task<List<UserChallenge>> GetRankedUserChallengesAsync(int challengeId)
    {
        return await _context.UserChallenges
            .Where(uc => uc.ChallengeId == challengeId && uc.ChallengeRole != UserChallengeRole.Organizer)
            .Include(uc => uc.User)
            .OrderByDescending(uc => uc.Progress)
            .ThenByDescending(uc => uc.CompletedTasks)
            .ThenBy(uc => uc.UpdatedAt)
            .ToListAsync();
    }

    public async Task<UserChallenge?> GetUserProgressAsync(int userId, int challengeId)
    {
        return await _context.UserChallenges
            .Include(uc => uc.User)
            .Include(uc => uc.Challenge)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);
    }

    public async Task<int> CountParticipantsAsync(int challengeId)
    {
        return await _context.UserChallenges
            .CountAsync(uc =>
                uc.ChallengeId == challengeId && uc.ChallengeRole == UserChallengeRole.Participant &&
                uc.Status == UserChallengeStatus.InProgress);
    }

    public async Task<HashSet<int>> GetCompletedUserIdsAsync(int challengeId)
    {
        var list = await _context.UserChallenges
            .Where(uc => uc.ChallengeId == challengeId && uc.Status == UserChallengeStatus.Completed)
            .Select(uc => uc.UserId)
            .ToListAsync();

        return [..list];
    }

    public async Task<UserChallenge?> GetOrganizerByChallengeIdAsync(int challengeId)
    {
        return await _context.UserChallenges
            .Include(uc => uc.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(uc => 
                uc.ChallengeId == challengeId && 
                uc.ChallengeRole == UserChallengeRole.Organizer);
    }
}