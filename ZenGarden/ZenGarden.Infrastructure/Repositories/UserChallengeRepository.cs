using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
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

    public async Task<List<UserChallenge>> GetRankedUserChallengesAsync(int challengeId)
    {
        return await _context.UserChallenges
            .Where(uc => uc.ChallengeId == challengeId)
            .Include(uc => uc.User)
            .OrderByDescending(uc => uc.Progress)
            .ThenByDescending(uc => uc.CompletedTasks)
            .ThenBy(uc => uc.UpdatedAt)
            .ToListAsync();
    }


    public async Task UpdateProgressAsync(int userId, int challengeId)
    {
        var userChallenge = await _context.UserChallenges
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);

        if (userChallenge == null) return;

        var totalTasks = await GetTotalTasksAsync(userId, challengeId);
        var completedTasks = await GetCompletedTasksAsync(userId, challengeId);

        userChallenge.CompletedTasks = completedTasks;
        userChallenge.Progress = totalTasks > 0 ? completedTasks * 100 / totalTasks : 0;
        userChallenge.UpdatedAt = DateTime.UtcNow;

        _context.UserChallenges.Update(userChallenge);
        await _context.SaveChangesAsync();
    }

    public async Task<UserChallenge?> GetUserProgressAsync(int userId, int challengeId)
    {
        return await _context.UserChallenges
            .Include(uc => uc.User)
            .Include(uc => uc.Challenge)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);
    }

    private async Task<int> GetTotalTasksAsync(int userId, int challengeId)
    {
        return await _context.Tasks
            .CountAsync(t => t.UserTree.UserId == userId &&
                             t.ChallengeTasks.Any(ct => ct.ChallengeId == challengeId));
    }

    private async Task<int> GetCompletedTasksAsync(int userId, int challengeId)
    {
        return await _context.Tasks
            .CountAsync(t => t.UserTree.UserId == userId &&
                             t.ChallengeTasks.Any(ct => ct.ChallengeId == challengeId) &&
                             t.Status == TasksStatus.Completed);
    }
}