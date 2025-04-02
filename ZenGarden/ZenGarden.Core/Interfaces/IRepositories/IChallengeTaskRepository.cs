using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IChallengeTaskRepository : IGenericRepository<ChallengeTask>
{
    Task<List<ChallengeTask>> GetTasksByChallengeIdAsync(int challengeId);
    Task<ChallengeTask?> GetByTaskIdAsync(int taskId);
}