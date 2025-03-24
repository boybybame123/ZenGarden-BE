using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserChallengeRepository : IGenericRepository<UserChallenge>
{
    Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId);
    Task<List<UserChallenge>> GetAllUsersInChallengeAsync(int challengeId);
    Task<List<UserChallenge>> GetRankedUserChallengesAsync(int challengeId);
    Task UpdateProgressAsync(int userId, int challengeId);
    Task<UserChallenge?> GetUserProgressAsync(int userId, int challengeId);
}