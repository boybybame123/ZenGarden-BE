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
            .Select(c => new Challenge
            {
                ChallengeId = c.ChallengeId,
                ChallengeName = c.ChallengeName,
                ChallengeType = c.ChallengeType,
                UserChallenges = c.UserChallenges,
                ChallengeTasks = c.ChallengeTasks
                    .Select(ct => new ChallengeTask
                    {
                        Tasks = new Tasks
                        {
                            TaskId = ct.TaskId,
                            TaskName = ct.Tasks!.TaskName,
                            Status = ct.Tasks.Status
                        }
                    }).ToList()
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Challenge?> GetByIdChallengeAsync(int id)
    {
        return await _context.Challenge
            .Where(c => c.ChallengeId == id)
            .Select(c => new Challenge
            {
                ChallengeId = c.ChallengeId,
                ChallengeName = c.ChallengeName,
                ChallengeType = c.ChallengeType,
                UserChallenges = c.UserChallenges,
                ChallengeTasks = c.ChallengeTasks
                    .Select(ct => new ChallengeTask
                    {
                        Tasks = new Tasks
                        {
                            TaskId = ct.TaskId,
                            TaskName = ct.Tasks!.TaskName,
                            Status = ct.Tasks.Status
                        }
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }
}