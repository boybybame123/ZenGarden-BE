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
            .AsNoTracking()
            .Include(c => c.UserChallenges)
            .Include(c => c.ChallengeTasks)
            .ThenInclude(ct => ct.Tasks)
            .Select(c => new Challenge
            {
                ChallengeId = c.ChallengeId,
                ChallengeName = c.ChallengeName,
                ChallengeType = c.ChallengeType,
                UserChallenges = c.UserChallenges.Select(uc => new UserChallenge
                {
                    UserId = uc.UserId,
                    ChallengeId = uc.ChallengeId
                }).ToList(),
                ChallengeTasks = c.ChallengeTasks
                    .Where(ct => ct.Tasks != null)
                    .Select(ct => new ChallengeTask
                    {
                        TaskId = ct.TaskId,
                        Tasks = ct.Tasks != null
                            ? new Tasks
                            {
                                TaskId = ct.Tasks.TaskId,
                                TaskName = ct.Tasks.TaskName,
                                Status = ct.Tasks.Status
                            }
                            : null
                    }).ToList()
            })
            .ToListAsync();
    }

    public async Task<Challenge?> GetByIdChallengeAsync(int id)
    {
        return await _context.Challenge
            .AsNoTracking()
            .Include(c => c.UserChallenges)
            .Include(c => c.ChallengeTasks)
            .ThenInclude(ct => ct.Tasks)
            .Where(c => c.ChallengeId == id)
            .Select(c => new Challenge
            {
                ChallengeId = c.ChallengeId,
                ChallengeName = c.ChallengeName,
                ChallengeType = c.ChallengeType,
                UserChallenges = c.UserChallenges.Select(uc => new UserChallenge
                {
                    UserId = uc.UserId,
                    ChallengeId = uc.ChallengeId
                }).ToList(),
                ChallengeTasks = c.ChallengeTasks
                    .Where(ct => ct.Tasks != null)
                    .Select(ct => new ChallengeTask
                    {
                        TaskId = ct.TaskId,
                        Tasks = ct.Tasks != null
                            ? new Tasks
                            {
                                TaskId = ct.Tasks.TaskId,
                                TaskName = ct.Tasks.TaskName,
                                Status = ct.Tasks.Status
                            }
                            : null
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }
}